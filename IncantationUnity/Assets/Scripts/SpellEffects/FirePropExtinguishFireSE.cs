using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePropExtinguishFireSE : SpellEffect
{
	public override SpellID SpellID => SpellID.ExtinguishFire;
	public new FireProp Target => Target;

	public override bool AreConditionsMet()
	{
		return Target.lit;
	}

	public override void Apply(float intensity)
	{
		base.Apply(intensity);
		Target.lit = false;
	}
}
