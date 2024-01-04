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

	public SpriteRenderer unexplodedSprite;
	public SpriteRenderer explodedSprite;

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
			goalSpellID == SpellID.Levitate ? vanished :
			false;
	}

	protected override void Awake()
	{
		base.Awake();

		exploded = false;
		levitating = false;
		vanished = false;
		raining = false;
	}

	protected override void Update()
	{
		base.Update();
		unexplodedSprite.enabled = !exploded;
		explodedSprite.enabled = exploded;
	}

	[System.Serializable]
	public new class LevitateSE : Card.LevitateSE
	{
		public new CardType Target => (CardType)base.Target;
		protected override Statum Levitating { get => Target.levitating; set => Target.levitating = value; }

		public override bool AreConditionsMet()
		{
			return !Target.levitating && !Target.vanished && !Target.exploded;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
		}
	}
	public LevitateSE levitateSE;

	[System.Serializable]
	public class MendSE : CardSE
	{
		public override SpellID SpellID => SpellID.Mend;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.exploded && !Target.vanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.exploded = false;
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
	public class ExplodeSE : CardSE
	{
		public override SpellID SpellID => SpellID.Explode;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.exploded;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.exploded = true;
		}
	}
	public ExplodeSE explodeSE;

	[System.Serializable]
	public class RefillSE : CardSE
	{
		public override SpellID SpellID => SpellID.Refill;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.exploded;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.exploded = true;
		}
	}
	public RefillSE refillSE;

	[System.Serializable]
	public class CreateFireSE : CardSE
	{
		public override SpellID SpellID => SpellID.CreateFire;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.exploded;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.exploded = true;
		}
	}
	public CreateFireSE createFireSE;

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
		}
	}
	public RainSE rainSE;
}
