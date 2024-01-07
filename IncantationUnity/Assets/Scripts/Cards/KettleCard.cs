using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = KettleCard;

public class KettleCard : Card
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
	public AudioSource boilingAudioSource;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Activate ? !vanished && GetTemp() > 0.02f :
			goalSpellID == SpellID.Deactivate ? !on || vanished || broken :
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Activate ? vanished || broken :
			goalSpellID == SpellID.Deactivate ? false :
			false;
	}

	public void SetTemp(float temp)
	{
		if (temp <= 0)
		{
			boilingAudioSource.Stop();
		}
		else
		{
			if (!boilingAudioSource.isPlaying)
			{
				boilingAudioSource.Play();
			}
			boilingAudioSource.time = Mathf.Lerp(0, boilingAudioSource.clip.length, temp);
		}
	}

	public float GetTemp()
	{
		return boilingAudioSource.isPlaying ? boilingAudioSource.time / boilingAudioSource.clip.length : 0;
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

		if (on && !boilingAudioSource.isPlaying)
		{
			boilingAudioSource.Play();
		}
		boilingAudioSource.volume = vanished ? 0 : 1;
		boilingAudioSource.pitch = on ? 1 : -1;

		// TODO: turn off at end of boiling sound, keep temp.
	}

	[System.Serializable]
	public class IgniteSE : CardSE
	{
		public override SpellID SpellID => SpellID.Ignite;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.SetTemp(0.8f);
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
			return !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.SetTemp(0.8f);
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
			return Target.GetTemp() > 0.0f && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.on = false;
			Target.SetTemp(Mathf.Min(Target.GetTemp() * 0.5f, 0.01f));
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
			if (Target.on)
			{
				// Turn off on leave stand.
				Target.on = false;
				SFXManager.Play(Target.turnOffSFX, parent: Target.transform);
			}
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
			SFXManager.Play(Target.turnOnSFX, parent: Target.transform);
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
			SFXManager.Play(Target.turnOffSFX, parent: Target.transform);
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
			Target.broken = true;
			Target.on = false;
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
