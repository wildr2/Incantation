using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = PlantpotCard;

public class PlantpotCard : Card
{
	public Statum sprouted;
	public Statum broken;
	public Statum levitating;
	public Statum vanished;
	public Statum raining;

	public SpriteRenderer unsproutedSprite;
	public SpriteRenderer sproutedSprite;
	public SpriteRenderer brokenUnsproutedSprite;
	public SpriteRenderer brokenSproutedSprite;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Grow ? sprouted && !vanished :
			goalSpellID == SpellID.Break ? broken && !vanished :
			goalSpellID == SpellID.Mend ? !broken && !vanished :
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Grow ? vanished :
			goalSpellID == SpellID.Break ? vanished :
			goalSpellID == SpellID.Mend ? vanished :
			false;
	}

	protected override void Awake()
	{
		base.Awake();

		sprouted = false;
		broken = goalSpellID == SpellID.Mend;
		levitating = false;
		vanished = false;
		raining = false;
	}

	protected override void Update()
	{
		base.Update();
		unsproutedSprite.enabled = !sprouted && !broken;
		sproutedSprite.enabled = sprouted && !broken;
		brokenUnsproutedSprite.enabled = !sprouted && broken;
		brokenSproutedSprite.enabled = sprouted && broken;
	}

	[System.Serializable]
	public class CreateFireSE : CardSE
	{
		public override SpellID SpellID => SpellID.CreateFire;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && Target.sprouted;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.sprouted = false;
		}
	}
	public CreateFireSE createFireSE;
	
	[System.Serializable]
	public new class LevitateSE : Card.LevitateSE
	{
		public new CardType Target => (CardType)base.Target;
		protected override Statum Levitating { get => Target.levitating; set => Target.levitating = value; }

		public override bool AreConditionsMet()
		{
			return !Target.vanished && (!Target.sprouted || (!Target.levitating && !Target.broken));
		}

		public override void Apply(float intensity)
		{
			if (!Target.sprouted)
			{
				Target.sprouted = true;
			}
			else
			{
				base.Apply(intensity);
			}
		}

		protected override void EndLevitation()
		{
			base.EndLevitation();
			// Break upon falling back down.
			Target.broken = true;
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
	public class ExplodeSE : CardSE
	{
		public override SpellID SpellID => SpellID.Explode;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && (!Target.broken || Target.sprouted);
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.broken = true;
			Target.sprouted = false;
			Target.levitating = false;
		}
	}
	public ExplodeSE explodeSE;

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
			Target.sprouted = true;
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
			return !Target.vanished && !Target.broken && !Target.sprouted;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.sprouted = true;
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
			return !Target.vanished && !Target.sprouted;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.sprouted = true;
		}
	}
	public GrowSE growSE;
}
