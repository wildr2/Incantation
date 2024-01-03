using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePropCreateFireSE : SpellEffect
{
	public override SpellID SpellID => SpellID.CreateFire;
	public new FireProp Target => Target;

	public override bool AreConditionsMet()
	{
		return !Target.lit;
	}

	public override void Apply(float intensity)
	{
		base.Apply(intensity);
		Target.lit = true;
	}
}
