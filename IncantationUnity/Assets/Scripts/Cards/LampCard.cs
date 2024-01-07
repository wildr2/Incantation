using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = LampCard;

public class LampCard : Card
{
	public Statum on;
	public Statum broken;
	public Statum levitating;
	public Statum vanished;

	public SpriteRenderer onSprite;
	public SpriteRenderer offSprite;
	public SpriteRenderer brokenSprite;

	public AudioClip turnOnSFX;
	public AudioClip turnOffSFX;
	public AudioSource buzzAudioSource;
	public AudioClip[] breakSFX;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Activate ? on && !vanished :
			goalSpellID == SpellID.Deactivate ? !on || vanished : 
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Activate ? vanished || broken :
			goalSpellID == SpellID.Deactivate ? false :
			false;
	}

	public void Break()
	{
		broken = true;
		on = false;
		levitating = false;
		SFXManager.Play(breakSFX);
	}

	protected override void Awake()
	{
		base.Awake();

		on = goalSpellID == SpellID.Deactivate;
		broken = false;
		levitating = false;
		vanished = false;
	}

	protected override void Update()
	{
		base.Update();
		onSprite.enabled = on;
		offSprite.enabled = !on && !broken;
		brokenSprite.enabled = broken;

		if ((on && !vanished) && !buzzAudioSource.isPlaying)
		{
			buzzAudioSource.Play();
		}
		else if ((!on || vanished) && buzzAudioSource.isPlaying)
		{
			buzzAudioSource.Stop();
		}
	}

	[System.Serializable]
	public class IgniteSE : CardSE
	{
		public override SpellID SpellID => SpellID.Ignite;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.broken;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.on = true;
		}
	}
	public IgniteSE igniteSE;

	[System.Serializable]
	public class ExplodeSE : CardSE
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
	public class ExtinguishSE : CardSE
	{
		public override SpellID SpellID => SpellID.Extinguish;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.on && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.on = false;
		}
	}
	public ExtinguishSE extinguishSE;
	
	[System.Serializable]
	public new class LevitateSE : Card.LevitateSE
	{
		public new CardType Target => (CardType)base.Target;
		protected override Statum Levitating { get => Target.levitating; set => Target.levitating = value; }

		public override bool AreConditionsMet()
		{
			return !Target.levitating && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
		}
	}
	public LevitateSE levitateSE;

	[System.Serializable]
	public class ActivateSE : CardSE
	{
		public override SpellID SpellID => SpellID.Activate;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.on && !Target.broken && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.on = true;
			SFXManager.Play(Target.turnOnSFX);
		}
	}
	public ActivateSE activateSE;

	[System.Serializable]
	public class DeactivateSE : CardSE
	{
		public override SpellID SpellID => SpellID.Deactivate;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.on && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.on = false;
			SFXManager.Play(Target.turnOffSFX);
		}
	}
	public DeactivateSE deactivateSE;

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
			Target.vanished = true;

			CardData.contentParent.SetActive(false);
		}
	}
	public VanishSE vanishSE;
}
