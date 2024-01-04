using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = GlassCard;

public class GlassCard : Card
{
	public Statum filledWithWater;
	public Statum filledWithPlant;
	public Statum broken;
	public Statum levitating;
	public Statum vanished;
	public Statum raining;

	public SpriteRenderer glassSprite;
	public SpriteRenderer brokenGlassSprite;
	public SpriteRenderer waterSprite;
	public SpriteRenderer plantSprite;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Refill ? (filledWithWater || filledWithPlant) && !vanished && !broken :
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

		filledWithWater = false;
		filledWithPlant = false;
		broken = false;
		levitating = false;
		vanished = false;
		raining = false;
	}

	protected override void Update()
	{
		base.Update();
		glassSprite.enabled = !broken;
		brokenGlassSprite.enabled = broken;
		waterSprite.enabled = filledWithWater;
		plantSprite.enabled = filledWithPlant;
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
			Target.filledWithPlant = false;
			Target.filledWithWater = false;
			Target.levitating = false;
		}
	}
	public ExplodeSE explodeSE;
	
	[System.Serializable]
	public new class LevitateSE : Card.LevitateSE
	{
		public new CardType Target => (CardType)base.Target;
		protected override Statum Levitating { get => Target.levitating; set => Target.levitating = value; }

		public override bool AreConditionsMet()
		{
			return !Target.levitating && !Target.vanished && !Target.broken;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
		}

		protected override void EndLevitation()
		{
			base.EndLevitation();
			// Break upon falling back down.
			Target.broken = true;
			Target.filledWithPlant = false;
			Target.filledWithWater = false;
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
			Target.filledWithWater = false;
			Target.filledWithPlant = false;
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
			if (!Target.vanished && !Target.broken)
			{
				Target.filledWithWater = true;
			}
		}
	}
	public RainSE rainSE;

	[System.Serializable]
	public class RefillSE : CardSE
	{
		public override SpellID SpellID => SpellID.Refill;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.broken && !Target.filledWithWater;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.filledWithWater = true;
		}
	}
	public RefillSE refillSE;

	[System.Serializable]
	public class GrowSE : CardSE
	{
		public override SpellID SpellID => SpellID.Grow;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.filledWithPlant && !Target.broken;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.filledWithPlant = true;
		}
	}
	public GrowSE growSE;
}
