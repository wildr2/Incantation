using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = DoorCard;

public class DoorCard : Card
{
	public Statum open;
	public Statum locked;
	public Statum exploded;

	public SpriteRenderer openSprite;
	public SpriteRenderer shutSprite;
	public SpriteRenderer explodedSprite;

	public AudioClip openSFX;
	public AudioClip shutSFX;
	public AudioClip lockSFX;
	public AudioClip unlockSFX;
	public AudioClip[] explodeSFX;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Unlock ? !locked :
			goalSpellID == SpellID.Lock ? locked :
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Unlock ? false :
			goalSpellID == SpellID.Lock ? exploded :
			false;
	}

	public void Open()
	{
		if (!open)
		{
			open = true;
			SFXManager.Play(openSFX, MixerGroup.Master, parent: transform);
		}
	}

	public void Shut()
	{
		if (open)
		{
			open = false;
			SFXManager.Play(shutSFX, MixerGroup.Master, parent: transform);
		}
	}

	public void Unlock()
	{
		if (locked)
		{
			locked = false;
			SFXManager.Play(unlockSFX, MixerGroup.Master, parent: transform);
		}
	}

	public void Lock()
	{
		// TODO: do all the error checking? state race conditions...
		if (!locked)
		{
			locked = true;
			SFXManager.Play(lockSFX, MixerGroup.Master, parent: transform);
		}
	}

	public void Explode()
	{
		exploded = true;
		open = true;
		locked = false;
		SFXManager.Play(explodeSFX, parent: transform);
	}

	protected override void Awake()
	{
		base.Awake();

		open = goalSpellID == SpellID.Lock;
		locked = goalSpellID == SpellID.Unlock;
		exploded = false;
	}

	protected override void Update()
	{
		base.Update();
		openSprite.enabled = open && !exploded;
		shutSprite.enabled = !open && !exploded;
		explodedSprite.enabled = exploded;
	}

	[System.Serializable]
	public class ActivateSE : CardSE
	{
		public override SpellID SpellID => SpellID.Activate;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.open && !Target.exploded && !Target.locked;
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
			return Target.open && !Target.exploded;
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

		public override bool AreConditionsMet()
		{
			return Target.locked;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Unlock();
			DoDelayed(Spell.openDelay, Target.Open);
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
			return !Target.locked && !Target.exploded;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Shut();
			DoDelayed(0.2f, Target.Lock);
		}
	}
	public LockSE lockSE;

	[System.Serializable]
	public class ExplodeSE : CardSE
	{
		public override SpellID SpellID => SpellID.Explode;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.exploded;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Explode();
		}
	}
	public ExplodeSE explodeSE;

	[System.Serializable]
	public class MendSE : CardSE
	{
		public override SpellID SpellID => SpellID.Mend;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.exploded;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.exploded = false;
			Target.locked = false;
			DoDelayed(spellCast.spell.EffectDuration, Target.Shut);
		}
	}
	public MendSE mendSE;
}
