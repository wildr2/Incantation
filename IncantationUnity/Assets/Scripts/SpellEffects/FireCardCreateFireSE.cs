using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCardCreateFireSE : CardSE
{
	public override SpellID SpellID => SpellID.CreateFire;
	public new FireCard Target => (FireCard)base.Target;

	public override bool AreConditionsMet()
	{
		return !Target.vanished && !Target.raining;
	}

	public override void Apply(float intensity)
	{
		base.Apply(intensity);
		Target.lit = true;
		Target.sprouted = false;
		Target.flamesGlowDuration = Mathf.Lerp(3.0f, 9.0f, intensity);
	}
}
