using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardType = FireCard;

public class FireCard : Card
{
	public Statum lit;
	public Statum levitating;
	public Statum vanished;
	public Statum raining;
	public Statum sprouted;

	public SpriteRenderer flamesBlackSprite;
	public SpriteRenderer flamesColorSprite;
	public SpriteRenderer flamesGlowSprite;
	[HideInInspector]
	public float flamesGlowIntensity;
	[HideInInspector]
	public float flamesGlowDuration;

	public override bool IsComplete()
	{
		return
			goalSpellID == SpellID.CreateFire ? lit && !vanished :
			goalSpellID == SpellID.ExtinguishFire ? !lit || vanished : 
			false;
	}

	public override bool IsSkippable()
	{
		return
			goalSpellID == SpellID.CreateFire ? vanished :
			goalSpellID == SpellID.ExtinguishFire ? false :
			false;
	}

	public override float GetGlowIntensity()
	{
		return flamesGlowIntensity;
	}

	protected override void Awake()
	{
		base.Awake();

		lit = goalSpellID == SpellID.ExtinguishFire;
		levitating = false;
		vanished = false;
		raining = false;
		sprouted = false;
	}

	protected override void Update()
	{
		base.Update();
		UpdateFlames();
	}

	private void UpdateFlames()
	{
		if (lit)
		{
			float flicker = Mathf.Lerp(1.0f, Mathf.PerlinNoise(Time.time * 8.0f, 0), 0.2f);
			float fade = Mathf.Lerp(1.0f, 0.0f, (Time.time - lit.time) / flamesGlowDuration);
			fade = 1.0f - Mathf.Pow(1.0f - fade, 4.0f);
			flamesGlowIntensity = fade * flicker;
		}
		else
		{
			flamesGlowIntensity = 0;
		}

		flamesBlackSprite.color = Util.SetAlpha(flamesBlackSprite.color, lit ? 1 : 0);
		flamesColorSprite.color = Util.SetAlpha(flamesColorSprite.color, flamesGlowIntensity);
		flamesGlowSprite.color = Util.SetAlpha(flamesGlowSprite.color, flamesGlowIntensity);
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
			Target.lit = true;
			Target.sprouted = false;
			Target.flamesGlowDuration = Mathf.Lerp(3.0f, 9.0f, intensity);
		}
	}
	public CreateFireSE createFireSE;

	[System.Serializable]
	public class ExplodeSE : CardSE
	{
		public override SpellID SpellID => SpellID.Explode;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.lit = true;
			Target.sprouted = false;
			Target.flamesGlowDuration = Mathf.Lerp(3.0f, 9.0f, intensity);
		}
	}
	public ExplodeSE explodeSE;
	
	[System.Serializable]
	public class ExtinguishFireSE : CardSE
	{
		public override SpellID SpellID => SpellID.ExtinguishFire;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.lit && !Target.vanished;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.lit = false;
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
	public class RainSE : CardSE
	{
		public override SpellID SpellID => SpellID.Rain;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.raining;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.raining = true;
			Target.sprouted = true;
			Target.lit = false; 
		}
	}
	public RainSE rainSE;

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

	[System.Serializable]
	public class GrowSE : CardSE
	{
		public override SpellID SpellID => SpellID.Grow;
		public new CardType Target => (CardType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.vanished && !Target.lit && !Target.sprouted;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.sprouted = true;
		}
	}
	public GrowSE growSE;
}
