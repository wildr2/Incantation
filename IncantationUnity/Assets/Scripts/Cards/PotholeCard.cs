using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = PotholeCard;

public class PotholeCard : Card
{
	public Statum mended;
	public Statum filledWithWater;
	public Statum raining;
	public Statum vanished;

	public SpriteRenderer potholeSprite;
	public SpriteRenderer potholeWithWaterSprite;
	public SpriteRenderer waterGlowSprite;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Mend ? mended || vanished :
			goalSpellID == SpellID.Rain ? raining :
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Mend ? false :
			goalSpellID == SpellID.Rain ? false :
			false;
	}

	protected override void Awake()
	{
		base.Awake();

		mended = false;
		filledWithWater = false;
		raining = false;
		vanished = false;
	}

	protected override void Update()
	{
		base.Update();
		potholeSprite.enabled = !mended && !filledWithWater;
		potholeWithWaterSprite.enabled = !mended && filledWithWater;
	}

	[System.Serializable]
	public class MendSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Mend;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.mended && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.mended = true;
			Target.filledWithWater = false;
		}
	}
	public MendSE mendSE;

	[System.Serializable]
	public class Fill : SpellEffect
	{
		public override SpellID SpellID => SpellID.Fill;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.mended && !Target.filledWithWater;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.mended = true;
		}
	}
	public Fill fill;

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
			if (!Target.vanished && !Target.filledWithWater)
			{
				Target.filledWithWater = true;
				Target.Glow(Target.waterGlowSprite);
			}
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
			return !Target.vanished && !Target.mended;
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
