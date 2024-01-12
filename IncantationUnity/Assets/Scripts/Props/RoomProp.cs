using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TargetType = RoomProp;

public class RoomProp : Prop
{
	bool stendarriiSummoned;

	[System.Serializable]
	public class SummonStendariiSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.SummonStendarii;
		public new TargetType Target => (TargetType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.stendarriiSummoned;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.stendarriiSummoned = true;
			FindObjectOfType<Music>().tmpMute = true;
			RoomLighting.Instance.TmpFlashRed();
		}
	}
	public SummonStendariiSE summonStendariiSE;
}
