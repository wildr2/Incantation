using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SpellEffect
{
	public abstract SpellID SpellID { get; }
	public SpellTarget Target { private set; get; }
	public virtual AudioClip[] OverrideSpellCastSFX => null;

	public AudioClip sfx;
	// Most recent spellCast applying this effect.
	protected SpellCast spellCast;
	// Most recently played sfx audioSource. Destroyed on done by SFXManager.
	protected AudioSource audioSource;

	public virtual void Init(SpellTarget target)
	{
		Target = target;
	}

	// A spell can only be cast if conditions are met for some matching effect of some target.
	public virtual bool AreConditionsMet()
	{
		return true;
	}

	public virtual void Apply(SpellCast spellCast)
	{
		this.spellCast = spellCast;

		Prop prop = Target as Prop;
		if (prop)
		{
			audioSource = SFXManager.Play(sfx, MixerGroup.Magic, prop.transform.position);
		}
		else
		{
			audioSource = SFXManager.Play(sfx, MixerGroup.Magic);
		}
	}

	public virtual void Update()
	{
	}

	protected virtual void Mute(bool mustSpellCastSFX = true)
	{
		if (audioSource)
		{
			audioSource.volume = 0.0f;
		}
		if (mustSpellCastSFX && spellCast != null && spellCast.audioSource)
		{
			spellCast.audioSource.volume = 0.0f;
		}
	}

	protected void DoDelayed(float delay, System.Action action)
	{
		Target.StartCoroutine(CoroutineUtil.DoAfterDelay(action, delay));
	}
}
