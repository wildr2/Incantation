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

	public SpriteRenderer closedSprite;
	public SpriteRenderer closedGlowSprite;
	public SpriteRenderer closedFlameSprite;
	public SpriteRenderer closedFlameGlowSprite;
	public SpriteRenderer openSprite;
	public SpriteRenderer openGlowSprite;
	public SpriteRenderer openFlameSprite;
	public SpriteRenderer openFlameGlowSprite;

	public AudioClip openSFX;
	public AudioClip closeSFX;

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

	public void SetFire()
	{
		burning = true;
		Glow(open ? openFlameGlowSprite : closedFlameGlowSprite);
	}

	public void Open()
	{
		open = true;
		SFXManager.Play(openSFX, MixerGroup.Master, parent: transform);
	}

	public void Close()
	{
		open = false;
		SFXManager.Play(closeSFX, MixerGroup.Master, parent: transform);
	}

	protected override void Awake()
	{
		base.Awake();

		open = goalSpellID == SpellID.Deactivate;

		closedGlowSprite.enabled = false;
		closedFlameGlowSprite.enabled = false;
		openGlowSprite.enabled = false;
		openFlameGlowSprite.enabled = false;
	}

	protected override void Update()
	{
		base.Update();

		closedSprite.enabled = !open;
		closedFlameSprite.enabled = !open && burning;
		openSprite.enabled = open;
		openFlameSprite.enabled = open && burning;
	}

	[System.Serializable]
	public class IgniteSE : SpellEffect
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
			Target.SetFire();
		}
	}
	public IgniteSE igniteSE;
	
	[System.Serializable]
	public class ExtinguishSE : SpellEffect
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
			Target.Open();
			Target.Glow(Target.openGlowSprite);
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
			return !Target.open && !Target.vanished;
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
			return Target.open && !Target.vanished;
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
		public override bool ShakeOnApply => false;

		public override bool AreConditionsMet()
		{
			return !Target.open && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Glow(Target.openGlowSprite);
			DoDelayed(Spell.openDelay, () =>
			{
				Target.Open();
				ShakeCard(Target, spellCast.intensity);
			});
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
			return Target.open && !Target.vanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Close();
			Target.Glow(Target.closedGlowSprite);
		}
	}
	public LockSE lockSE;

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
