using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SpellEffect
{
	public abstract SpellID SpellID { get; }
	public SpellTarget Target { private set; get; }
	public virtual float TargetPriorityOffset => 0;
	public virtual AudioClip[] OverrideSpellCastSFX => null;
	public virtual bool ShakeOnApply => true;
	public CommonCardData CommonCardData => Target as Card ? ((Card)Target).commonCardData : null;

	// Most recent spellCast applying this effect.
	protected SpellCast spellCast;

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
		if (ShakeOnApply)
		{
			Card card = Target as Card;
			if (card)
			{
				ShakeCard(card, spellCast.intensity);
			}
		}
	}

	public virtual void Update()
	{
	}

	protected virtual void Mute(bool mustSpellCastSFX = true)
	{
		if (mustSpellCastSFX && spellCast != null && spellCast.audioSource)
		{
			spellCast.audioSource.volume = 0.0f;
		}
	}

	protected void DoDelayed(float delay, System.Action action)
	{
		Target.StartCoroutine(CoroutineUtil.DoAfterDelay(action, delay));
	}

	protected virtual void ShakeCard(Card card, float intensity)
	{
		card.Shake(Util.Map(0, 1, 0.5f, 1.0f, intensity));
	}
}
