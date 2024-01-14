using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = FireCard;

public class FireCard : Card
{
	public Statum lit;
	public Statum vanished;
	public Statum raining;

	public SpriteRenderer unlitFireSprite;
	public SpriteRenderer litFireSprite;
	public SpriteRenderer fireGlowSprite;

	public AudioSource fireSFXSource;

	public override bool Raining => raining;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Ignite ? lit && !vanished :
			goalSpellID == SpellID.Extinguish ? !lit || vanished : 
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Ignite ? vanished :
			goalSpellID == SpellID.Extinguish ? false :
			false;
	}

	protected override void Awake()
	{
		base.Awake();

		lit = goalSpellID == SpellID.Extinguish;
		vanished = false;
		raining = false;

		fireGlowSprite.enabled = false;
	}

	protected override void Update()
	{
		base.Update();
		UpdateFlames();
		fireSFXSource.volume = lit ? 1.0f : 0.0f;
	}

	private void UpdateFlames()
	{
		unlitFireSprite.enabled = !lit;
		litFireSprite.enabled = lit;
	}

	[System.Serializable]
	public class IgniteSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Ignite;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.lit = true;
			Target.Glow(Target.fireGlowSprite);
		}
	}
	public IgniteSE igniteSE;

	[System.Serializable]
	public class ExplodeSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Explode;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.lit = true;
			Target.Glow(Target.fireGlowSprite);
		}
	}
	public ExplodeSE explodeSE;
	
	[System.Serializable]
	public class ExtinguishSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Extinguish;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.lit && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.lit = false;
		}
	}
	public ExtinguishSE extinguishSE;

	[System.Serializable]
	public class RainSE : CardRainSE
	{
		public new CardType Target => (CardType)base.Target;
		protected override Statum Raining { get => Target.raining; set => Target.raining = value; }
		public AudioClip fireHiss;

		public override bool AreConditionsMet()
		{
			return !Target.raining;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
		}

		public override void Update()
		{
			base.Update();
			if (Raining && Target.lit && Time.time - Raining.time > 1.0f && Time.time - Target.lit.time > 0.5f)
			{
				ExtinguishFire();
			}
		}

		private void ExtinguishFire()
		{
			Target.lit = false;
			SFXManager.Play(fireHiss, parent: Target.transform);
		}
	}
	public RainSE rainSE;

	[System.Serializable]
	public class VanishSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Vanish;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.vanished = true;

			CommonCardData.contentParent.SetActive(false);
		}
	}
	public VanishSE vanishSE;
}
