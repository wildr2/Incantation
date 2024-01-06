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
	public AudioClip castSFX;

	public IncantationDef incantationDef; 

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

	public bool TryCastSpell(SpellTarget target, float intensity)
	{
		SpellEffect effect = System.Array.Find(target.SpellEffects, e => e.SpellID == SpellID && e.AreConditionsMet());
		if (effect == null)
		{
			return false;
		}

		// Magic!
		effect.Apply(intensity);

		Prop prop = target as Prop;
		if (prop)
		{
			SFXManager.Play(castSFX, MixerGroup.Magic, prop.transform.position);
		}
		else
		{
			SFXManager.Play(castSFX, MixerGroup.Magic);
		}

		return true;
	}
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
