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
	public SpriteRenderer openGlowSprite;
	public SpriteRenderer closedSprite;
	public SpriteRenderer closedGlowSprite;
	public SpriteRenderer explodedSprite;
	public SpriteRenderer explodedGlowSprite;

	public AudioClip openSFX;
	public AudioClip closeSFX;
	public AudioClip lockSFX;
	public AudioClip unlockSFX;
	public AudioClip[] explodeSFX;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Unlock ? !locked || open :
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

	public void Close()
	{
		if (open)
		{
			open = false;
			SFXManager.Play(closeSFX, MixerGroup.Master, parent: transform);
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
		SFXManager.Play(explodeSFX, parent: transform);
	}

	protected override void Awake()
	{
		base.Awake();

		open = goalSpellID == SpellID.Lock;
		locked = goalSpellID == SpellID.Unlock;
		exploded = false;

		openGlowSprite.enabled = false;
		closedGlowSprite.enabled = false;
		explodedGlowSprite.enabled = false;
	}

	protected override void Update()
	{
		base.Update();
		openSprite.enabled = open && !exploded;
		closedSprite.enabled = !open && !exploded;
		explodedSprite.enabled = exploded;
	}

	[System.Serializable]
	public class ActivateSE : SpellEffect
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
			Target.Glow(Target.openGlowSprite);
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
			return Target.open && !Target.exploded;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Close();
			Target.Glow(Target.closedGlowSprite);
		}
	}
	public DeactivateSE deactivateSE;

	[System.Serializable]
	public class UnlockSE : SpellEffect
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
			Target.Glow(Target.closedGlowSprite);
			DoDelayed(Spell.unlockDelay, Target.Unlock);
			DoDelayed(Spell.openDelay, Open);
		}

		private void Open()
		{
			Target.Open();
			Target.Glow(Target.openGlowSprite);
		}
	}
	public UnlockSE unlockSE;

	[System.Serializable]
	public class LockSE : SpellEffect
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
			Target.Close();
			Target.Glow(Target.closedGlowSprite);
			DoDelayed(0.2f, Lock);
		}

		public void Lock()
		{
			Target.Lock();
			Target.Glow(Target.closedGlowSprite);
		}
	}
	public LockSE lockSE;

	[System.Serializable]
	public class ExplodeSE : SpellEffect
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
			//Target.Glow(Target.explodedGlowSprite);
		}
	}
	public ExplodeSE explodeSE;

	[System.Serializable]
	public class MendSE : SpellEffect
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
			Target.Glow(Target.openGlowSprite);
			DoDelayed(spellCast.spell.EffectDuration, Close);
		}

		private void Close()
		{
			Target.Close();
			//Target.Glow(Target.closedGlowSprite);
		}
	}
	public MendSE mendSE;
}
