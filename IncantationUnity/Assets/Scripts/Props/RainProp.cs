using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TargetType = RainProp;

public class RainProp : Prop
{
	public Statum raining;

	[System.Serializable]
	public class RainSE : BaseRainSE
	{
		public new TargetType Target => (TargetType)base.Target;
		protected override Statum Raining { get => Target.raining; set => Target.raining = value; }

		public AudioClip rainingSFX;
		protected override AudioClip[] AudioClips => new AudioClip[] { rainingSFX };

		public override bool AreConditionsMet()
		{
			return !Raining;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
		}
	}
	public RainSE rainSE;
}
