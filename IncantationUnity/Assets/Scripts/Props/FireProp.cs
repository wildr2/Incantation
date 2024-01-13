using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TargetType = FireProp;

public class FireProp : Prop
{
	public Statum lit;
	private AudioSource fireSFXSource;

	protected override void Awake()
	{
		base.Awake();
		fireSFXSource = GetComponent<AudioSource>();
		lit = true;
	}

	protected override void Update()
	{
		base.Update();
		fireSFXSource.volume = lit ? 1.0f : 0.0f;
	}

	[System.Serializable]
	public class IgniteSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Ignite;
		public new TargetType Target => (TargetType)base.Target;

		// When dark, higher priority than card and lamp.
		public override float TargetPriorityOffset => RoomLighting.Instance.Brightness < 0.5f ? 2.5f : base.TargetPriorityOffset;

		public override bool AreConditionsMet()
		{
			return !Target.lit;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.lit = true;
		}
	}
	public IgniteSE igniteSE;

	[System.Serializable]
	public class ExtinguishSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Extinguish;
		public new TargetType Target => (TargetType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.lit;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.lit = false;
		}
	}
	public ExtinguishSE extinguishSE;
}
