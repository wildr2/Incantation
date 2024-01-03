using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = DungeonCard;

public class DungeonCard : Card
{
	public Statum torchLit;
	public Statum chrVanished;

	public SpriteRenderer chrSprite;
	public SpriteRenderer litTorchSprite;
	public SpriteRenderer unlitTorchSprite;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.Vanish ? chrVanished || !torchLit :
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.Vanish ? false :
			false;
	}

	protected override void Awake()
	{
		base.Awake();

		torchLit = true;
		chrVanished = false;
	}

	protected override void Update()
	{
		base.Update();
		chrSprite.enabled = !chrVanished && torchLit;
		litTorchSprite.enabled = torchLit;
		unlitTorchSprite.enabled = !torchLit;
	}

	[System.Serializable]
	public class VanishSE : CardSE
	{
		public override SpellID SpellID => SpellID.Vanish;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.chrVanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.chrVanished = true;
		}
	}
	public VanishSE vanishSE;

	[System.Serializable]
	public class CreateFireSE : CardSE
	{
		public override SpellID SpellID => SpellID.CreateFire;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return true;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.torchLit = true;
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
			return Target.torchLit;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.torchLit = false;
		}
	}
	public ExtinguishFireSE extinguishFireSE;

	[System.Serializable]
	public class ActivateSE : CardSE
	{
		public override SpellID SpellID => SpellID.Activate;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.torchLit;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.torchLit = true;
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
			return Target.torchLit;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.torchLit = false;
		}
	}
	public DeactivateSE deactivateSE;

	[System.Serializable]
	public class ExplodeSE : CardSE
	{
		public override SpellID SpellID => SpellID.Explode;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return true;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.torchLit = true;
		}
	}
	public ExplodeSE explodeSE;
}