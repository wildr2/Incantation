using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : SpellTarget
{
	[System.Serializable]
	public abstract class PropSE : SpellEffect
	{
		public new Prop Target => (Prop)base.Target;

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
		}
	}
}
