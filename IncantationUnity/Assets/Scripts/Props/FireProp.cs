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
	public class CreateFireSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.CreateFire;
		public new PropType Target => (PropType)base.Target;

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
	public CreateFireSE createFireSE;

	[System.Serializable]
	public class ExtinguishFireSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.ExtinguishFire;
		public new PropType Target => (PropType)base.Target;

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
	public ExtinguishFireSE extinguishFireSE;
}
