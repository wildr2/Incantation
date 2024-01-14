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

	public SpriteRenderer lampSprite;
	public SpriteRenderer lampGlowSprite;
	public SpriteRenderer bulbGlowSprite;
	public SpriteRenderer bulbOffSprite;
	public SpriteRenderer bulbBrokenSprite;
	public SpriteRenderer bulbBrokenGlowSprite;

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
		SFXManager.Play(breakSFX, parent: transform);
	}

	protected override void Awake()
	{
		base.Awake();

		on = goalSpellID == SpellID.Deactivate;

		lampGlowSprite.enabled = false;
		bulbGlowSprite.enabled = false;
		bulbBrokenGlowSprite.enabled = false;
	}

	protected override void Update()
	{
		base.Update();

		bulbOffSprite.enabled = !on && !broken;
		bulbBrokenSprite.enabled = broken;

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
	public class IgniteSE : SpellEffect
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
			Target.Glow(Target.bulbGlowSprite);
		}
	}
	public IgniteSE igniteSE;

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
			Target.Glow(Target.bulbBrokenGlowSprite);
		}
	}
	public ExplodeSE explodeSE;
	
	[System.Serializable]
	public class ExtinguishSE : SpellEffect
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
	public class LevitateSE : CardLevitateSE
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
			Target.Glow(Target.lampGlowSprite);
		}
	}
	public LevitateSE levitateSE;

	[System.Serializable]
	public class ActivateSE : SpellEffect
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
			SFXManager.Play(Target.turnOnSFX, parent: Target.transform);
			Target.Glow(Target.bulbGlowSprite);
		}
	}
	public ActivateSE activateSE;

	[System.Serializable]
	public class DeactivateSE : SpellEffect
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
			SFXManager.Play(Target.turnOffSFX, parent: Target.transform);
		}
	}
	public DeactivateSE deactivateSE;

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
			Target.Glow(Target.bulbBrokenGlowSprite);
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
			Target.Glow(Target.bulbGlowSprite);
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
}
