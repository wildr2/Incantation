using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PropType = FireProp;

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
		public new PropType Target => (PropType)base.Target;

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
		public new PropType Target => (PropType)base.Target;

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
