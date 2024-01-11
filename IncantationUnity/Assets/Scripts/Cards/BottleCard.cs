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

	public SpriteRenderer bottleSprite;
	public SpriteRenderer bottleGlowSprite;
	public SpriteRenderer bottleBrokenSprite;
	public SpriteRenderer bottleBrokenGlowSprite;
	public SpriteRenderer liquidGlowSprite;

	public AudioClip[] breakSFX;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Fill ? filledWithWine && !vanished :
			goalSpellID == SpellID.Break ? broken :
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Fill ? vanished || broken :
			goalSpellID == SpellID.Break ? false :
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

		bottleGlowSprite.enabled = false;
		bottleBrokenGlowSprite.enabled = false;
		liquidGlowSprite.enabled = false;
	}

	protected override void Update()
	{
		base.Update();
		bottleSprite.enabled = !broken;
		bottleBrokenSprite.enabled = broken;
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
			Target.Glow(Target.bottleBrokenGlowSprite);
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
			Target.Glow(Target.bottleGlowSprite);
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
			Target.Glow(Target.bottleBrokenGlowSprite);
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
			Target.Glow(Target.bottleGlowSprite);
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
			Target.Glow(Target.liquidGlowSprite);
		}
	}
	public Fill fill;
}
