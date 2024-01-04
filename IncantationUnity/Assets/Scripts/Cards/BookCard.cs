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
	public class CreateFireSE : CardSE
	{
		public override SpellID SpellID => SpellID.CreateFire;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.burning = true;
		}
	}
	public CreateFireSE createFireSE;
	
	[System.Serializable]
	public class ExtinguishFireSE : CardSE
	{
		public override SpellID SpellID => SpellID.ExtinguishFire;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.burning && !Target.vanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.burning = false;
		}
	}
	public ExtinguishFireSE extinguishFireSE;
	
	[System.Serializable]
	public new class LevitateSE : Card.LevitateSE
	{
		public new CardType Target => (CardType)base.Target;
		protected override Statum Levitating { get => Target.levitating; set => Target.levitating = value; }

		public override bool AreConditionsMet()
		{
			return !Target.levitating && !Target.vanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
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
			return Target.open && !Target.vanished;
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
			return !Target.open && !Target.vanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
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
			return Target.open && !Target.vanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.open = false;
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

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.vanished = true;

			CardData.contentParent.SetActive(false);
		}
	}
	public VanishSE vanishSE;
}
