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

	public SpriteRenderer emptySprite;
	public SpriteRenderer filledWithWaterSprite;

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
		emptySprite.enabled = !mended;
		filledWithWaterSprite.enabled = filledWithWater;
	}

	[System.Serializable]
	public class MendSE : CardSE
	{
		public override SpellID SpellID => SpellID.Mend;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.mended && !Target.vanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.mended = true;
			Target.filledWithWater = false;
		}
	}
	public MendSE mendSE;

	[System.Serializable]
	public class RefillSE : CardSE
	{
		public override SpellID SpellID => SpellID.Refill;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.mended && !Target.filledWithWater;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.mended = true;
		}
	}
	public RefillSE refillSE;

	[System.Serializable]
	public class RainSE : CardSE
	{
		public override SpellID SpellID => SpellID.Rain;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.raining;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.raining = true;
			if (!Target.vanished && !Target.filledWithWater)
			{
				Target.filledWithWater = true;
			}
		}
	}
	public RainSE rainSE;

	[System.Serializable]
	public class VanishSE : CardSE
	{
		public override SpellID SpellID => SpellID.Vanish;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.mended;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.vanished = true;

			CardData.contentParent.SetActive(false);
		}
	}
	public VanishSE vanishSE;
}
