using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGenericSE : CardSE
{
	public override SpellID SpellID => SpellID.Generic;

	public override bool AreConditionsMet()
	{
		return true;
	}

	public override void Apply(float intensity)
	{
		base.Apply(intensity);
	}

	protected override void Shake(float intensity)
	{
		// Shake the card only a small amount.
		Target.Shake(Util.Map(0, 1, 0.0f, 0.3f, intensity));
	}
}
