using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PropType = FireProp;

public class FireProp : Prop
{
	public CreateFireSE createFireSE;
	public ExtinguishFireSE extinguishFireSE;

	public Statum lit;
	private AudioSource fireSFXSource;

	protected override void Awake()
	{
		base.Awake();
		fireSFXSource = GetComponent<AudioSource>();
		lit = true;
	}

	private void Update()
	{
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
}
