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

	public AudioClip[] breakSFX;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Fill ? filledWithWine && !vanished :
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Fill ? vanished || broken :
			false;
	}

	public void Break()
	{
		broken = true;
		filledWithWine = false;
		levitating = false;
		SFXManager.Play(breakSFX, parent: transform);
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
	public class ExplodeSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Explode;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.broken;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Break();
		}
	}
	public ExplodeSE explodeSE;
	
	[System.Serializable]
	public class LevitateSE : CardLevitateSE
	{
		public new CardType Target => (CardType)base.Target;
		protected override Statum Levitating { get => Target.levitating; set => Target.levitating = value; }

		public override bool AreConditionsMet()
		{
			return !Target.levitating && !Target.vanished && !Target.broken;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
		}

		protected override void EndLevitation()
		{
			base.EndLevitation();
			// Break upon falling back down.
			Target.Break();
		}
	}
	public LevitateSE levitateSE;

	[System.Serializable]
	public class BreakSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Break;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.broken && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Break();
		}
	}
	public BreakSE breakSE;

	[System.Serializable]
	public class MendSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Mend;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.broken && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.broken = false;
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
	public class Fill : SpellEffect
	{
		public override SpellID SpellID => SpellID.Fill;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.broken && !Target.filledWithWine;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.filledWithWine = true;
		}
	}
	public Fill fill;
}
