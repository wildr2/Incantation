using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellID
{
	Generic,
	CreateFire,
	ExtinguishFire,
	Activate,
	Deactivate,
	Refill,
	Break,
	Unlock,
	Lock,
	Explode,
	Vanish,
	MakeRain,
	Grow,
	Mend,
	Levitate,
	ConjureRock,
}

[System.Serializable]
public class Spell
{
	[HideInInspector]
	public SpellID spellID;
	public AudioClip castSFX;

	public Spell(SpellID id)
	{
		spellID = id;
	}

	public bool TryCastSpell(SpellTarget target, float intensity)
	{
		SpellEffect effect = System.Array.Find(target.SpellEffects, e => e.SpellID == spellID && e.AreConditionsMet());
		if (effect == null)
		{
			return false;
		}

		// Magic!
		effect.Apply(intensity);
		SFXManager.Play(castSFX, MixerGroup.Magic);

		return true;
	}
}
