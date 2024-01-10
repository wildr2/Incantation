using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class CardLevitateSE : SpellEffect
{
	public override SpellID SpellID => SpellID.Levitate;
	protected abstract Statum Levitating { get; set; }
	protected virtual Transform TransformToMove => CommonCardData.contentParent.transform;
	public AudioClip landSFX;
	protected bool visuallyLevitating;

	public override bool AreConditionsMet()
	{
		return !Levitating;
	}

	public override void Apply(SpellCast spellCast)
	{
		base.Apply(spellCast);
		StartLevitating();
	}

	public override void Update()
	{
		base.Update();
		if (visuallyLevitating)
		{
			if (!Levitating || spellCast.CastTime >= spellCast.spell.effectEndTime)
			{
				// Interrupted or duration up.
				EndLevitation();
			}
		}
	}

	protected virtual void StartLevitating()
	{
		Levitating = true;
		visuallyLevitating = true;
		SetContentPosition(true);
	}

	protected virtual void EndLevitation()
	{
		Levitating = false;
		visuallyLevitating = false;
		SetContentPosition(false);

		if (spellCast.audioSource)
		{
			spellCast.audioSource.Stop();
		}

		if (CanLandLevitation())
		{
			SFXManager.Play(landSFX, MixerGroup.Master, parent: Target.transform);
		}
	}

	protected virtual bool CanLandLevitation()
	{
		return true;
	}

	protected virtual void SetContentPosition(bool levitated)
	{
		TransformToMove.localPosition = levitated ? CommonCardData.levitatePos : Vector2.zero;
	}
}