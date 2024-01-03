using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = BottleCard;

public class BottleCard : Card
{
	public Statum filledWithWine;
	public Statum broken;
	public Statum levitating;
	public Statum vanished;

	public SpriteRenderer glassSprite;
	public SpriteRenderer brokenGlassSprite;
	public SpriteRenderer wineSprite;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Refill ? filledWithWine && !vanished :
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Refill ? vanished || broken :
			false;
	}

	protected override void Awake()
	{
		base.Awake();

		filledWithWine = false;
		broken = false;
		levitating = false;
		vanished = false;
	}

	protected override void Update()
	{
		base.Update();
		glassSprite.enabled = !broken;
		brokenGlassSprite.enabled = broken;
		wineSprite.enabled = filledWithWine;
	}

	[System.Serializable]
	public class ExplodeSE : CardSE
	{
		public override SpellID SpellID => SpellID.Explode;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.broken;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.broken = true;
			Target.filledWithWine = false;
			Target.levitating = false;
		}
	}
	public ExplodeSE explodeSE;
	
	[System.Serializable]
	public class LevitateSE : CardSE
	{
		public override SpellID SpellID => SpellID.Levitate;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.levitating && !Target.vanished && !Target.broken;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.levitating = true;

			CardData.contentParent.transform.localPosition = CardData.levitatePos;
		}
	}
	public LevitateSE levitateSE;

	[System.Serializable]
	public class BreakSE : CardSE
	{
		public override SpellID SpellID => SpellID.Break;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.broken && !Target.vanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.broken = true;
			Target.filledWithWine = false;
			Target.levitating = false;
		}
	}
	public BreakSE breakSE;

	[System.Serializable]
	public class MendSE : CardSE
	{
		public override SpellID SpellID => SpellID.Mend;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.broken && !Target.vanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.broken = false;
		}
	}
	public MendSE mendSE;

	[System.Serializable]
	public class VanishSE : CardSE
	{
		public override SpellID SpellID => SpellID.Vanish;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.vanished = true;

			CardData.contentParent.SetActive(false);
		}
	}
	public VanishSE vanishSE;

	[System.Serializable]
	public class RefillSE : CardSE
	{
		public override SpellID SpellID => SpellID.Refill;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.broken && !Target.filledWithWine;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.filledWithWine = true;
		}
	}
	public RefillSE refillSE;
}
