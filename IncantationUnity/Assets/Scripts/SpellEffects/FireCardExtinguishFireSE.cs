using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCardExtinguishFireSE : CardSE
{
	public override SpellID SpellID => SpellID.ExtinguishFire;
	public new FireCard Target => Target;

	public override bool AreConditionsMet()
	{
		return Target.lit && !Target.vanished;
	}

	public override void Apply(float intensity)
	{
		base.Apply(intensity);
		Target.lit = false;
	}
}
