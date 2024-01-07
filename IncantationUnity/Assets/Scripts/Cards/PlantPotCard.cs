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

	public AudioClip[] breakSFX;
	public AudioClip growingSFX;

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

	public void Break()
	{
		broken = true;
		levitating = false;
		SFXManager.Play(breakSFX);
	}

	public void Grow()
	{
		if (!sprouted)
		{
			sprouted = true;
			SFXManager.Play(growingSFX);
		}
	}

	protected override void Awake()
	{
		base.Awake();

		sprouted = goalSpellID == SpellID.Grow ? false : Random.value < 0.5f;
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
	public class IgniteSE : CardSE
	{
		public override SpellID SpellID => SpellID.Ignite;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && Target.sprouted;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.sprouted = false;
		}
	}
	public IgniteSE igniteSE;
	
	[System.Serializable]
	public new class LevitateSE : Card.LevitateSE
	{
		public new CardType Target => (CardType)base.Target;
		protected override Statum Levitating { get => Target.levitating; set => Target.levitating = value; }

		public override AudioClip[] OverrideSpellCastSFX => !Target.sprouted ? Player.Instance.levitateSpell.castSFXIntro : null;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && (!Target.sprouted || (!Target.levitating && !Target.broken));
		}

		public override void Apply(SpellCast spellCast)
		{
			if (!Target.sprouted)
			{
				Target.Grow();
			}
			else
			{
				base.Apply(spellCast);
			}
		}

		protected override void EndLevitation()
		{
			// Sometimes break upon falling back down.
			if (Random.value < 0.5f)
			{
				Target.Break();
			}
			base.EndLevitation();
		}

		protected override bool CanLandLevitation()
		{
			return !Target.broken;
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

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Break();
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

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
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

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Break();
			Target.sprouted = false;
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

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			if (Target.sprouted)
			{
				Target.sprouted = false;
			}
			else
			{
				Target.vanished = true;
				CardData.contentParent.SetActive(false);
			}
		}
	}
	public VanishSE vanishSE;

	[System.Serializable]
	public new class RainSE : Card.RainSE
	{
		public new CardType Target => (CardType)base.Target;
		protected override Statum Raining { get => Target.raining; set => Target.raining = value; }
		protected override AudioClip[] AudioClips => !Target.vanished && !Target.broken ?
			new AudioClip[] { CardData.rainingSFX, CardData.rainingOnGlassSFX } :
			base.AudioClips;

		public override bool AreConditionsMet()
		{
			return !Target.raining;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.raining = true;
			Target.Grow();
		}
	}
	public RainSE rainSE;

	[System.Serializable]
	public class Fill : CardSE
	{
		public override SpellID SpellID => SpellID.Fill;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.broken && !Target.sprouted;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Grow();
		}
	}
	public Fill fill;

	[System.Serializable]
	public class GrowSE : CardSE
	{
		public override SpellID SpellID => SpellID.Grow;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.sprouted;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Grow();
		}
	}
	public GrowSE growSE;
}
