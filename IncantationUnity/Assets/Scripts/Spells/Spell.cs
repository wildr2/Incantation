using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

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

	public Spell(SpellID id)
	{
		SpellID = id;
	}

	public void Init(IncantationDef incantationDef)
	{
		this.IncantationDef = incantationDef;
	}

	public bool CheckIncantation(string incantation, IncantationCircumstance circumstances)
	{
		return IncantationDef == null || IncantationDef.Passes(incantation, circumstances);
	}

	public bool CheckDebugIncantation(string incantation, IncantationCircumstance circumstances)
	{
		return incantation == debugIncantation && IncantationDef.PassesCircumstances(circumstances);
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

	public IncantationDef(IncantationDefConfig config)
	{
		int n = config.custom ? 
			config.customRuleTypes.Length : 
			Random.value < 0.2f ? 2 : 1;
		rules = new IncantationRule[n];

		for (int i = 0; i < n; ++i)
		{
			if (config.custom)
			{
				rules[i] = IncantationRule.Random(config.customRuleTypes[i]);
			}
			else
			{ 
				rules[i] = i == 0 ? IncantationRule.Random() : IncantationRule.Random(IncantationRuleType.ContainsLetter);
			}	
		}

		if (config.custom)
		{
			requiredCircumstances = config.customCircumstances;
		}
		else
		{
			requiredCircumstances = 0;
		}
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

	public bool IsEquivalent(IncantationDef other)
	{
		if (rules.Length != other.rules.Length)
		{
			return false;
		}
		foreach (IncantationRule rule in rules)
		{
			foreach (IncantationRule otherRule in other.rules)
			{
				if (!rule.IsEquivalent(otherRule))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static IncantationDef CreateUnique(IncantationDefConfig config, List<IncantationDef> defs)
	{
		IncantationDef newDef = null;
		for (int i = 0; i < 100 && newDef == null; ++i)
		{
			newDef = new IncantationDef(config);
			foreach (IncantationDef def in defs)
			{
				if (newDef.IsEquivalent(def))
				{
					newDef = null;
					break;
				}
			}
		}
		if (newDef == null)
		{
			Debug.LogError("Failed to create unique IncatationDef.");
		}
		return newDef;
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
					desc += string.Format("When the sky weeps\n");
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
				return string.Format("Incantation starts with {0}", letter);
			case IncantationRuleType.ContainsLetter:
				return string.Format("Incantation contains {0}", letter);
			case IncantationRuleType.EndsWithLetter:
				return string.Format("Incantation ends with {0}", letter);
			case IncantationRuleType.NLettersLong:
				return string.Format("Incantation contains {0} letters", n);
			case IncantationRuleType.LongWord:
				return string.Format("Incantation contains a long word", n);
			case IncantationRuleType.TwoWords:
				return string.Format("Incantation is two words", n);
			default:
				break;
		}
		return "";
	}

	public bool IsEquivalent(IncantationRule other)
	{
		return ruleType == other.ruleType && letter == other.letter && n == other.n;
	}
}

[System.Serializable]
public class IncantationDefConfig
{
	public bool custom;
	public IncantationCircumstance customCircumstances;
	public IncantationRuleType[] customRuleTypes;
}
