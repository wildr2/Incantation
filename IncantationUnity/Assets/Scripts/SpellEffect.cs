using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpellEffect : MonoBehaviour
{
	public abstract SpellID SpellID { get; }
	public SpellTarget Target { private set; get; }
	public AudioClip sfx;

	// A spell can only be cast if conditions are met for some matching effect of some target.
	public virtual bool AreConditionsMet()
	{
		return true;
	}

	public virtual void Apply(float intensity)
	{
		SFXManager.Play(sfx, MixerGroup.Magic);
	}

	protected virtual void Awake()
	{
		Target = GetComponent<SpellTarget>();
	}
}
