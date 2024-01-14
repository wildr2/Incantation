using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

// Add at end to not break data.
public enum SpellID
{
	Activate,
	Break,
	Ignite,
	Deactivate,
	Explode,
	Extinguish,
	Generic,
	Grow,
	Levitate,
	Lock,
	Mend,
	Rain,
	Fill,
	Unlock,
	Vanish,
	SummonStendarii,
}

[System.Serializable]
public class Spell
{
	public string fancyName;
	public int priority;
	public IncantationDefConfig incantationDefConfig;
	public string debugIncantation;
	public AudioClip[] castSFX;
	// Seconds after the cast start time after which the spell effect is applied.
	public float effectStartTime;
	// Seconds after the cast start time after which the spell effect should end, or -1 if not applicable.
	public float effectEndTime = -1;

	public SpellID SpellID { get; private set; }
	public IncantationDef IncantationDef { get; private set; }
	[HideInInspector]
	public bool seen;

	public float EffectDuration => effectEndTime >= 0 ? effectEndTime - effectStartTime : 0.0f;

	public static bool CompatibleWithRequiredCircumstances(SpellID spellID, IncantationCircumstance circumstances)
	{
		// XXX: It would be a problem if all various spells needed to create the Dark circumstance required the Dark circumstance.
		//      Avoiding that problem by only assigning each circumstance to a single spell. Also see ArePrerequisitesLearned.
		switch (spellID)
		{
			case SpellID.Rain:
				return !circumstances.HasFlag(IncantationCircumstance.Raining);
			case SpellID.Extinguish:
				return
					!circumstances.HasFlag(IncantationCircumstance.Dark) &&
					!circumstances.HasFlag(IncantationCircumstance.PitchBlack);
			default:
				return true;
		}
	}

	public Spell(SpellID id)
	{
		SpellID = id;
	}

	public void Init(IncantationDef incantationDef)
	{
		IncantationDef = incantationDef;
	}

	public bool CheckIncantation(string incantation, IncantationCircumstance circumstances)
	{
		return IncantationDef.Passes(incantation, circumstances);
	}

	public bool CheckDebugIncantation(string incantation, IncantationCircumstance circumstances)
	{
		return 
			incantation == debugIncantation &&
			(DebugSettings.Instance.debugIncantationsBypassCirumstances || IncantationDef.PassesCircumstances(circumstances));
	}

	public bool IsTargettable(SpellTarget target)
	{
		SpellEffect effect = System.Array.Find(target.SpellEffects, e => e.SpellID == SpellID && e.AreConditionsMet());
		return effect != null;
	}

	public bool IsTargettable(SpellTarget[] targets)
	{
		foreach (SpellTarget target in targets)
		{
			if (IsTargettable(target))
			{
				return true;
			}
		}
		return false;
	}

	public float GetHighestSpellTargetPriority(SpellTarget[] targets)
	{
		float priority = -1;
		foreach (SpellTarget target in targets)
		{
			if (IsTargettable(target))
			{
				priority = Mathf.Max(priority, target.Priority);
			}
		}
		return priority;
	}

	public SpellCast TryCastSpell(SpellTarget target, float intensity)
	{
		SpellEffect effect = System.Array.Find(target.SpellEffects, e => e.SpellID == SpellID && e.AreConditionsMet());
		if (effect == null)
		{
			return null;
		}

		SpellCast spellCast = new SpellCast()
		{
			spell = this,
			target = target,
			effect = effect,
			castStartTime = Time.time,
			intensity = intensity,
		};

		AudioClip[] sfx = effect.OverrideSpellCastSFX != null ? effect.OverrideSpellCastSFX  : castSFX;
		Prop prop = target as Prop;
		if (prop)
		{
			spellCast.audioSource = SFXManager.Play(sfx, MixerGroup.Magic, prop.transform.position, parent: target.transform);
		}
		else
		{
			spellCast.audioSource = SFXManager.Play(sfx, MixerGroup.Magic, parent: target.transform);
		}

		return spellCast;
	}

	public string GetDescription()
	{
		string desc = "";
		desc = string.Format("Spell of {0}\n\n", fancyName).ToUpper();
		desc += IncantationDef.GetDescription();
		return desc;
	}

	// Alphabetical ascending.
	public class NameComparer : IComparer<Spell>
	{
		public int Compare(Spell a, Spell b)
		{
			return a.SpellID.ToString().CompareTo(b.SpellID.ToString());
		}
	}
}

public class SpellCast
{
	public Spell spell;
	public SpellTarget target;
	public SpellEffect effect;
	public float castStartTime;
	public float intensity;
	// Destroyed on done by SFXManager.
	public AudioSource audioSource;

	public float EffectStartTime => castStartTime + spell.effectStartTime;
	public float CastTime => Time.time - castStartTime;
	public float EffectTime => Time.time - EffectStartTime;
}

public enum IncantationRuleType
{
	StartsWithLetter,
	ContainsLetter,
	EndsWithLetter,
	NLettersLong,
	LongWord,
	TwoWords,
}

[System.Flags]
public enum IncantationCircumstance
{
	Dark = 1 << 0,
	PitchBlack = 1 << 1,
	Raining = 1 << 2,
}

public class IncantationDef
{
	public IncantationRule[] rules;
	public IncantationCircumstance requiredCircumstances;
	public int ConditionCount => rules.Length + Util.CountFlags(requiredCircumstances);

	public IncantationDef()
	{
	}

	public IncantationDef(IncantationDefConfig config, IList<IncantationCircumstance> possibleCircumstances)
	{
		// Rules.
		int n = config.custom ? 
			config.customRuleTypes.Length : 
			Random.value < 0.2f ? 2 : 1;

		List<IncantationRule> rulesList = new List<IncantationRule>(n);
		for (int i = 0; i < n; ++i)
		{
			// TODO: Fine for now.
			const int tries = 20;
			for (int t = 0; t < tries; ++t)
			{
				IncantationRule rule;
				if (config.custom)
				{
					rule = config.customRuleLetters.Length > 0 ?
						IncantationRule.Custom(config.customRuleTypes[i], config.customRuleLetters[i]) :
						IncantationRule.Random(config.customRuleTypes[i]);
				}
				else
				{
					rule = i == 0 ? IncantationRule.Random() : IncantationRule.Random(IncantationRuleType.ContainsLetter);
				}
				if (rule.IsUnique(rulesList))
				{
					rulesList.Add(rule);
					break;
				}
			}
		}
		rules = rulesList.ToArray();
		if (rulesList.Count != n)
		{
			Debug.LogError(string.Format("Failed to create {0} unique incantation def rules.", n));
		}


		// Circumstances.
		if (config.custom)
		{
			requiredCircumstances = config.customCircumstances;
		}
		else
		{
			// XXX: input spell configs are shuffled + we want to assign all possible circumstances once.
			if (possibleCircumstances.Count > 0)
			{
				requiredCircumstances = possibleCircumstances[Random.Range(0, possibleCircumstances.Count)];
			}
			else
			{
				requiredCircumstances = 0;
			}
		}
	}

	public static IncantationDef TestDef(int rulesCount)
	{
		IncantationDef def = new IncantationDef();
		def.rules = new IncantationRule[rulesCount];
		for (int i = 0; i < rulesCount; ++i)
		{
			def.rules[i] = new IncantationRule();
		}
		return def;
	}

	public bool PassesCircumstances(IncantationCircumstance circumstances)
	{
		foreach (IncantationCircumstance c in System.Enum.GetValues(circumstances.GetType()))
		{
			if (requiredCircumstances.HasFlag(c) && !circumstances.HasFlag(c))
			{
				return false;
			}
		}
		return true;
	}

	public bool Passes(string incantation, IncantationCircumstance circumstances)
	{
		if (!PassesCircumstances(circumstances))
		{
			return false;
		}
		foreach (IncantationRule rule in rules)
		{
			if (!rule.Passes(incantation))
			{
				return false;
			}
		}
		return true;
	}

	public bool ArePrerequisitesLearned(IEnumerable<Spell> learned)
	{
		if (requiredCircumstances.HasFlag(IncantationCircumstance.Dark))
		{
			Spell spell = learned.FirstOrDefault((s) =>
			{
				bool rightSpell =
					s.SpellID == SpellID.Deactivate ||
					s.SpellID == SpellID.Extinguish ||
					s.SpellID == SpellID.Break ||
					s.SpellID == SpellID.Explode;
				bool rightSpellCircumstances = !s.IncantationDef.CircumstancesInclude(IncantationCircumstance.Dark);
				return rightSpell && rightSpellCircumstances;
			});
			if (spell == null)
			{
				return false;
			}
		}
		if (requiredCircumstances.HasFlag(IncantationCircumstance.PitchBlack))
		{
			if (learned.FirstOrDefault(s => s.SpellID == SpellID.Extinguish) == null)
			{
				return false;
			}
		}
		if (requiredCircumstances.HasFlag(IncantationCircumstance.Raining))
		{
			if (learned.FirstOrDefault(s => s.SpellID == SpellID.Rain) == null)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEquivalent(IncantationDef other)
	{
		if (other.requiredCircumstances != requiredCircumstances)
		{
			return false;
		}
		if (rules.Length != other.rules.Length)
		{
			return false;
		}
		foreach (IncantationRule rule in rules)
		{
			if (System.Array.Find(other.rules, r => r.IsEquivalent(rule)) == null)
			{
				return false;
			}
		}
		return true;
	}

	// Does this def include the conditions of a given other def and possibly have additional conditions.
	// If true, anything that passes this def also passes other, but not necessarily vice versa.
	public bool Includes(IncantationDef other)
	{
		if (!CircumstancesInclude(other.requiredCircumstances))
		{
			return false;
		}
		foreach (IncantationRule otherRule in other.rules)
		{
			if (rules.Length == 0)
			{
				return false;
			}
			if (System.Array.Find(rules, r => r.Includes(otherRule)) == null)
			{
				return false;
			}
		}
		return true;
	}

	// Do these circumstances include the conditions of a given other rule and possibly have additional conditions.
	// If true, anything that passes these circumstances also passes other, but not necessarily vice versa.
	public bool CircumstancesInclude(IncantationCircumstance other)
	{
		// TODO: clearer if circumstances were not flags... possibly make Circumstance incantation rule type
		foreach (IncantationCircumstance c in System.Enum.GetValues(other.GetType()))
		{
			if (other.HasFlag(c))
			{
				if (requiredCircumstances.HasFlag(c))
				{
					continue;
				}
				if (c == IncantationCircumstance.Dark && requiredCircumstances.HasFlag(IncantationCircumstance.PitchBlack))
				{
					continue;
				}
				return false;
			}
		}
		return true;
	}

	public static IList<IncantationDef> CreateUniqueDefs(IList<SpellID> spellIDs, IList<IncantationDefConfig> configs)
	{
		List<IncantationDef> defs = new List<IncantationDef>(configs.Count);
		List<IncantationCircumstance> possibleCircumstances = new List<IncantationCircumstance>() 
		{
			IncantationCircumstance.Dark,
			IncantationCircumstance.Raining,
		};
		foreach (IncantationDefConfig config in configs)
		{
			// TODO: Fine for now.
			IncantationDef def = null;
			const int tries = 100;
			for (int i = 0; i < tries; ++i)
			{
				def = new IncantationDef(config, possibleCircumstances);
				if (def.IsUnique(defs))
				{
					break;
				}
			}

			defs.Add(def);

			if (def != null)
			{
				possibleCircumstances.Remove(def.requiredCircumstances);
			}
			else
			{
				Debug.LogError("Failed to create unique incantation def.");
			}
		}
		return defs;
	}

	public bool IsUnique(IList<IncantationDef> others)
	{
		foreach (IncantationDef other in others)
		{
			if (IsEquivalent(other))
			{
				return false;
			}
		}
		return true;
	}

	public string GetDescription()
	{
		string desc = "";
		foreach (IncantationRule rule in rules)
		{
			desc += string.Format("{0}\n", rule.GetDescription());
		}
		desc += GetCircumstancesDescription();
		return desc;
	}

	public string GetCircumstancesDescription()
	{
		string desc = "";
		foreach (IncantationCircumstance c in System.Enum.GetValues(requiredCircumstances.GetType()))
		{
			if (!requiredCircumstances.HasFlag(c))
			{
				continue;
			}
			switch (c)
			{
				case IncantationCircumstance.Dark:
					desc += string.Format("In darkness\n");
					break;
				case IncantationCircumstance.PitchBlack:
					desc += string.Format("In utter darkness\n");
					break;
				case IncantationCircumstance.Raining:
					desc += string.Format("In the rain\n");
					break;
				default:
					break;
			}
		}
		desc.TrimEnd(System.Environment.NewLine.ToCharArray());
		return desc;
	}
}

public class IncantationRule
{
	public IncantationRuleType ruleType;
	public char letter;
	public int n;

	public static IncantationRule Random()
	{
		List<IncantationRuleType> possibleRuleTypes = new List<IncantationRuleType>()
		{
			IncantationRuleType.ContainsLetter,
			IncantationRuleType.NLettersLong,
			IncantationRuleType.LongWord,
			IncantationRuleType.TwoWords,
		};
		IncantationRuleType ruleType = possibleRuleTypes[UnityEngine.Random.Range(0, possibleRuleTypes.Count)];
		return Random(ruleType);
	}

	public static IncantationRule Random(IncantationRuleType type)
	{
		IncantationRule rule = new IncantationRule();
		rule.ruleType = type;

		if (rule.ruleType == IncantationRuleType.ContainsLetter || 
			rule.ruleType == IncantationRuleType.StartsWithLetter ||
			rule.ruleType == IncantationRuleType.EndsWithLetter)
		{
			rule.letter = Util.RandomLetter();
		}
		if (rule.ruleType == IncantationRuleType.NLettersLong)
		{
			rule.n = UnityEngine.Random.Range(3, 6);
		}

		return rule;
	}

	public static IncantationRule Custom(IncantationRuleType type, char letter)
	{
		IncantationRule rule = new IncantationRule();
		rule.ruleType = type;

		if (rule.ruleType == IncantationRuleType.ContainsLetter || 
			rule.ruleType == IncantationRuleType.StartsWithLetter ||
			rule.ruleType == IncantationRuleType.EndsWithLetter)
		{
			rule.letter = letter;
		}
		else
		{
			Debug.LogError("Invalid custom rule.");
		}

		return rule;
	}

	public bool Passes(string incantation)
	{
		switch (ruleType)
		{
			case IncantationRuleType.StartsWithLetter:
				return incantation.StartsWith(letter);
			case IncantationRuleType.ContainsLetter:
				return incantation.Contains(letter);
			case IncantationRuleType.EndsWithLetter:
				return incantation.EndsWith(letter);
			case IncantationRuleType.NLettersLong:
				return incantation.Length == n;
			case IncantationRuleType.LongWord:
				string[] words = incantation.Split(' ');
				foreach (string word in words)
				{
					if (word.Length >= 7)
					{
						return true;
					}
				}
				return false;
			case IncantationRuleType.TwoWords:
				Regex regex = new Regex(@"^\w+\s+\w+$");
				return regex.Matches(incantation).Count > 0;
			default:
				break;
		}
		return false;
	}

	public string GetDescription()
	{
		switch (ruleType)
		{
			case IncantationRuleType.StartsWithLetter:
				return string.Format("Incantations start with '{0}'", letter);
			case IncantationRuleType.ContainsLetter:
				return string.Format("Incantations contain '{0}'", letter);
			case IncantationRuleType.EndsWithLetter:
				return string.Format("Incantations end with '{0}'", letter);
			case IncantationRuleType.NLettersLong:
				return string.Format("Incantations contain a word of {0} letters", n);
			case IncantationRuleType.LongWord:
				return string.Format("Incantations contain a long word", n);
			case IncantationRuleType.TwoWords:
				return string.Format("Incantations are two words long", n);
			default:
				break;
		}
		return "";
	}

	public bool IsEquivalent(IncantationRule other)
	{
		return ruleType == other.ruleType && letter == other.letter && n == other.n;
	}

	// Does this rule include the conditions of a given other rule and possibly have additional conditions.
	// If true, anything that passes this rule also passes other, but not necessarily vice versa.
	public bool Includes(IncantationRule other)
	{
		if (IsEquivalent(other))
		{
			return true;
		}
		if (ruleType == IncantationRuleType.StartsWithLetter || ruleType == IncantationRuleType.EndsWithLetter)
		{
			return other.ruleType == IncantationRuleType.ContainsLetter && letter == other.letter;
		}
		return false;
	}

	public bool IsUnique(IList<IncantationRule> others)
	{
		foreach (IncantationRule other in others)
		{
			if (IsEquivalent(other))
			{
				return false;
			}
		}
		return true;
	}
}

[System.Serializable]
public class IncantationDefConfig
{
	public bool custom;
	public IncantationCircumstance customCircumstances;
	public IncantationRuleType[] customRuleTypes;
	public char[] customRuleLetters;
}
