using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = BookCard;

public class BookCard : Card
{
	public Statum open;
	public Statum burning;
	public Statum levitating;
	public Statum vanished;

	public SpriteRenderer openSprite;
	public SpriteRenderer shutSprite;
	public SpriteRenderer burningOpenSprite;
	public SpriteRenderer burningShutSprite;

	public AudioClip openSFX;
	public AudioClip shutSFX;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Activate ? open && !vanished :
			goalSpellID == SpellID.Deactivate ? !open && !vanished :
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Activate ? vanished :
			goalSpellID == SpellID.Deactivate ? vanished :
			false;
	}

	public void Open()
	{
		open = true;
		SFXManager.Play(openSFX, MixerGroup.Master, parent: transform);
	}

	public void Shut()
	{
		open = false;
		SFXManager.Play(shutSFX, MixerGroup.Master, parent: transform);
	}

	protected override void Awake()
	{
		base.Awake();

		open = goalSpellID == SpellID.Deactivate;
		burning = false;
		levitating = false;
		vanished = false;
	}

	protected override void Update()
	{
		base.Update();
		openSprite.enabled = open && !burning;
		shutSprite.enabled = !open && !burning;
		burningOpenSprite.enabled = open && burning;
		burningShutSprite.enabled = !open && burning;
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
			Target.burning = true;
		}
	}
	public IgniteSE igniteSE;
	
	[System.Serializable]
	public class ExtinguishSE : CardSE
	{
		public override SpellID SpellID => SpellID.Extinguish;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.burning && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.burning = false;
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
			return !Target.open && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Open();
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
			return Target.open && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Shut();
		}
	}
	public DeactivateSE deactivateSE;

	[System.Serializable]
	public class UnlockSE : CardSE
	{
		public override SpellID SpellID => SpellID.Unlock;
		public UnlockSpell Spell => (UnlockSpell)spellCast.spell;
		public new CardType Target => (CardType)base.Target;
		public override bool ShakeOnApply => false;

		public override bool AreConditionsMet()
		{
			return !Target.open && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			DoDelayed(Spell.openDelay, () =>
			{
				Target.Open();
				Shake(spellCast.intensity);
			});
		}
	}
	public UnlockSE unlockSE;

	[System.Serializable]
	public class LockSE : CardSE
	{
		public override SpellID SpellID => SpellID.Lock;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.open && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Shut();
		}
	}
	public LockSE lockSE;

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
