using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = BarrelCard;

public class BarrelCard: Card
{
	public Statum exploded;
	public Statum levitating;
	public Statum vanished;
	public Statum raining;

	public SpriteRenderer barrelSprite;
	public SpriteRenderer barrelGlowSprite;
	public SpriteRenderer rubbleSprite;
	public SpriteRenderer rubbleGlowSprite;

	public AudioClip[] explodeSFX;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Explode ? exploded && !vanished :
			goalSpellID == SpellID.Levitate ? levitating && !vanished : 
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Explode ? vanished :
			goalSpellID == SpellID.Levitate ? vanished || exploded :
			false;
	}

	public void Explode()
	{
		exploded = true;
		levitating = false;
		SFXManager.Play(explodeSFX, parent: transform);
	}

	protected override void Awake()
	{
		base.Awake();

		exploded = false;
		levitating = false;
		vanished = false;
		raining = false;

		barrelGlowSprite.enabled = false;
		rubbleGlowSprite.enabled = false;
	}

	protected override void Update()
	{
		base.Update();

		barrelSprite.enabled = !exploded;
		rubbleSprite.enabled = exploded;
	}

	[System.Serializable]
	public class LevitateSE : CardLevitateSE
	{
		public new CardType Target => (CardType)base.Target;
		protected override Statum Levitating { get => Target.levitating; set => Target.levitating = value; }

		public override bool AreConditionsMet()
		{
			return !Target.levitating && !Target.vanished && !Target.exploded;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Glow(Target.barrelGlowSprite);
		}

		protected override bool CanLandLevitation()
		{
			return !Target.exploded && !Target.vanished;
		}

		// TODO: not scalable...
		public override void Update()
		{
			base.Update();
			if (Target.vanished)
			{
				Mute();
			}
		}
	}
	public LevitateSE levitateSE;

	[System.Serializable]
	public class MendSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Mend;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.exploded && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.exploded = false;
			Target.Glow(Target.barrelGlowSprite);
		}
	}
	public MendSE mendSE;

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

	[System.Serializable]
	public class ExplodeSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Explode;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.exploded;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Explode();
			Target.Glow(Target.rubbleGlowSprite);
		}
	}
	public ExplodeSE explodeSE;

	[System.Serializable]
	public class Fill : SpellEffect
	{
		public override SpellID SpellID => SpellID.Fill;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.exploded;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Explode();
			Target.Glow(Target.rubbleGlowSprite);
		}
	}
	public Fill fill;

	[System.Serializable]
	public class IgniteSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Ignite;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.exploded;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Explode();
			Target.Glow(Target.rubbleGlowSprite);
		}
	}
	public IgniteSE igniteSE;

	[System.Serializable]
	public class RainSE : CardRainSE
	{
		public new CardType Target => (CardType)base.Target;
		protected override Statum Raining { get => Target.raining; set => Target.raining = value; }

		public override bool AreConditionsMet()
		{
			return !Target.raining;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
		}
	}
	public RainSE rainSE;
}
