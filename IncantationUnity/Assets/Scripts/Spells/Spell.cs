using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

[System.Serializable]
public class Spell
{
	public SpellID SpellID { get; private set; }
	public string debugIncantation;
	public AudioClip[] castSFX;
	// Seconds after the cast start time after which the spell effect is applied.
	public float effectStartTime;
	// Seconds after the cast start time after which the spell effect should end, or -1 if not applicable.
	public float effectEndTime = -1;
	public IncantationDef incantationDef;

	public float EffectDuration => effectEndTime >= 0 ? effectEndTime - effectStartTime : 0.0f;

	public Spell(SpellID id)
	{
		SpellID = id;
	}

	public void Init()
	{
		incantationDef = SpellID == SpellID.Generic ? null : new IncantationDef();
	}

	public bool CheckIncantation(string incantation)
	{
		return incantationDef == null || incantationDef.Passes(incantation);
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

	public int GetHighestSpellTargetPriority(SpellTarget[] targets)
	{
		int priority = -1;
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
			spellCast.audioSource = SFXManager.Play(sfx, MixerGroup.Magic, prop.transform.position);
		}
		else
		{
			spellCast.audioSource = SFXManager.Play(sfx, MixerGroup.Magic);
		}

		return spellCast;
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
}

public class IncantationDef
{
	public IncantationRule[] rules;

	public IncantationDef()
	{
		rules = new IncantationRule[1];
		rules[0] = new IncantationRule();
	}

	public bool Passes(string incantation)
	{
		foreach (IncantationRule rule in rules)
		{
			if (!rule.Passes(incantation))
			{
				return false;
			}
		}
		return true;
	}
}

public class IncantationRule
{
	public IncantationRuleType ruleType;
	public char letter;

	public IncantationRule()
	{
		//ruleType = (IncantationRuleType)Random.Range(0, Util.GetEnumCount<IncantationRuleType>());
		ruleType = IncantationRuleType.ContainsLetter;
		letter = Util.RandomLetter();
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
			default:
				break;
		}
		return "";
	}
}
