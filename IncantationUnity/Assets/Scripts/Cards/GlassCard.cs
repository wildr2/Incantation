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
	public SpriteRenderer glassGlowSprite;
	public SpriteRenderer glassBrokenSprite;
	public SpriteRenderer glassBrokenGlowSprite;
	public SpriteRenderer plantSprite;
	public SpriteRenderer plantGlowSprite;
	public SpriteRenderer liquidSprite;
	public SpriteRenderer liquidGlowSprite;
	public SpriteRenderer liquidPlantSprite;
	public SpriteRenderer liquidPlantGlowSprite;

	public AudioClip[] breakSFX;
	public AudioClip growingSFX;

	public override bool Raining => raining;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Fill ? (filledWithWater || filledWithPlant) && !vanished && !broken :
			goalSpellID == SpellID.Grow ? filledWithPlant && !vanished :
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Fill ? vanished || broken :
			goalSpellID == SpellID.Grow ? vanished || broken :
			false;
	}

	public void Break()
	{
		broken = true;
		filledWithPlant = false;
		filledWithWater = false;
		levitating = false;
		SFXManager.Play(breakSFX, parent: transform);
	}

	public void Grow()
	{
		if (!filledWithPlant)
		{
			filledWithPlant = true;
			SFXManager.Play(growingSFX, parent: transform);
		}
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

		glassGlowSprite.enabled = false;
		glassBrokenGlowSprite.enabled = false;
		plantGlowSprite.enabled = false;
		liquidGlowSprite.enabled = false;
		liquidPlantGlowSprite.enabled = false;
	}

	protected override void Update()
	{
		base.Update();
		glassSprite.enabled = !broken;
		glassBrokenSprite.enabled = broken;
		plantSprite.enabled = filledWithPlant;
		liquidSprite.enabled = filledWithWater && !filledWithPlant;
		liquidPlantSprite.enabled = filledWithWater && filledWithPlant;
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
			Target.Glow(Target.glassBrokenGlowSprite);
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
			Target.Glow(Target.glassGlowSprite);
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
			Target.Glow(Target.glassBrokenGlowSprite);
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
			Target.Glow(Target.glassGlowSprite);
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
	public class RainSE : CardRainSE
	{
		public new CardType Target => (CardType)base.Target;
		protected override Statum Raining { get => Target.raining; set => Target.raining = value; }
		protected override AudioClip[] AudioClips => !Target.vanished && !Target.broken ?
			new AudioClip[] { CommonCardData.rainingSFX, CommonCardData.rainingOnGlassSFX } :
			base.AudioClips;

		public override bool AreConditionsMet()
		{
			return !Target.raining;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			if (!Target.vanished && !Target.broken)
			{
				Target.filledWithWater = true;
				//Target.Glow(Target.filledWithPlant ? Target.liquidPlantGlowSprite : Target.liquidGlowSprite);
				Target.Glow(Target.liquidGlowSprite);
			}
		}
	}
	public RainSE rainSE;

	[System.Serializable]
	public class Fill : SpellEffect
	{
		public override SpellID SpellID => SpellID.Fill;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.broken && !Target.filledWithWater;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.filledWithWater = true;
			//Target.Glow(Target.filledWithPlant ? Target.liquidPlantGlowSprite : Target.liquidGlowSprite);
			Target.Glow(Target.liquidGlowSprite);
		}
	}
	public Fill fill;

	[System.Serializable]
	public class GrowSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Grow;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.filledWithPlant && !Target.broken;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Grow();
			Target.Glow(Target.plantGlowSprite);
		}
	}
	public GrowSE growSE;
}
