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

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.open = true;
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

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.open = false;
		}
	}
	public DeactivateSE deactivateSE;

	[System.Serializable]
	public class UnlockSE : CardSE
	{
		public override SpellID SpellID => SpellID.Unlock;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.locked;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.locked = false;
			Target.open = true;
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

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.open = false;
			Target.locked = true;
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

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.exploded = true;
			Target.open = true;
			Target.locked = false;
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

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.exploded = false;
			Target.open = false;
			Target.locked = false;
		}
	}
	public MendSE mendSE;
}
