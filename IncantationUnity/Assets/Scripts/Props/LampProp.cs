using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TargetType = LampProp;

public class LampProp : Prop
{
	public Statum on;
	public Statum broken;
	
	public AudioClip turnOnSFX;
	public AudioClip turnOffSFX;
	public AudioClip[] breakSFX;

	protected override void Awake()
	{
		base.Awake();
		on = true;
		broken = false;
	}

	public void Break()
	{
		broken = true;
		on = false;
		SFXManager.Play(breakSFX, parent: transform);
	}

	[System.Serializable]
	public class IgniteSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Ignite;
		public new TargetType Target => (TargetType)base.Target;
		// Higher priority than card when off.
		public override float TargetPriorityOffset => !Target.on ? 2 : base.TargetPriorityOffset;

		public override bool AreConditionsMet()
		{
			return !Target.broken && !Target.on;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.on = true;
		}
	}
	public IgniteSE igniteSE;

	[System.Serializable]
	public class ExplodeSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Explode;
		public new TargetType Target => (TargetType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.broken;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Break();
		}
	}
	public ExplodeSE explodeSE;
	
	[System.Serializable]
	public class ExtinguishSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Extinguish;
		public new TargetType Target => (TargetType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.on;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.on = false;
		}
	}
	public ExtinguishSE extinguishSE;
	
	[System.Serializable]
	public class ActivateSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Activate;
		public new TargetType Target => (TargetType)base.Target;
		// Higher priority than card when off.
		public override float TargetPriorityOffset => !Target.on ? 2 : base.TargetPriorityOffset;

		public override bool AreConditionsMet()
		{
			return !Target.on && !Target.broken;
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
	public class DeactivateSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Deactivate;
		public new TargetType Target => (TargetType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.on;
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
		public new TargetType Target => (TargetType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.broken;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Break();
		}
	}
	public BreakSE breakSE;

	[System.Serializable]
	public class MendSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Mend;
		public new TargetType Target => (TargetType)base.Target;
		public override float TargetPriorityOffset => !Target.on ? 2 : base.TargetPriorityOffset;

		public override bool AreConditionsMet()
		{
			return Target.broken;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.broken = false;
		}
	}
	public MendSE mendSE;
}
