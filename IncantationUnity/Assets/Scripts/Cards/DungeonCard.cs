using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = DungeonCard;

public class DungeonCard : Card
{
	public Statum torchLit;
	public Statum chrVanished;

	public SpriteRenderer personSprite;
	public SpriteRenderer torchSprite;
	public SpriteRenderer torchFlameSprite;
	public SpriteRenderer torchFlameGlowSprite;

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

		torchFlameGlowSprite.enabled = false;
	}

	protected override void Update()
	{
		base.Update();
		personSprite.enabled = !chrVanished && torchLit;
		torchFlameSprite.enabled = torchLit;
	}

	[System.Serializable]
	public class VanishSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Vanish;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.chrVanished;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.chrVanished = true;
		}
	}
	public VanishSE vanishSE;

	[System.Serializable]
	public class IgniteSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Ignite;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return true;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.torchLit = true;
			Target.Glow(Target.torchFlameGlowSprite);
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
			return Target.torchLit;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.torchLit = false;
		}
	}
	public ExtinguishSE extinguishSE;

	[System.Serializable]
	public class ActivateSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Activate;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.torchLit;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.torchLit = true;
			Target.Glow(Target.torchFlameGlowSprite);
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
			return Target.torchLit;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.torchLit = false;
		}
	}
	public DeactivateSE deactivateSE;

	[System.Serializable]
	public class ExplodeSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Explode;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return true;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.torchLit = true;
			Target.Glow(Target.torchFlameGlowSprite);
		}
	}
	public ExplodeSE explodeSE;
}
