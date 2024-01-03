using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellID
{
	Activate,
	Break,
	ConjureRock,
	CreateFire,
	Deactivate,
	Explode,
	ExtinguishFire,
	Generic,
	Grow,
	Levitate,
	Lock,
	Mend,
	Rain,
	Refill,
	Unlock,
	Vanish,
}

[System.Serializable]
public class Spell
{
	public SpellID SpellID { get; private set; }
	public string debugIncantation;
	public AudioClip castSFX;

	public Spell(SpellID id)
	{
		SpellID = id;
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
