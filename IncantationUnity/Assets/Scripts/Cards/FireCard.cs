using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FireCardCreateFireSE))]
[RequireComponent(typeof(FireCardExtinguishFireSE))]
public class FireCard : Card
{
	public Statum lit;
	public Statum woodBroken;
	public Statum levitating;
	public Statum vanished;
	public Statum raining;
	public Statum sprouted;

	public SpriteRenderer flamesBlack;
	public SpriteRenderer flamesColor;
	public SpriteRenderer flamesGlow;
	[HideInInspector]
	public float flamesGlowIntensity;
	[HideInInspector]
	public float flamesGlowDuration;

	public override float GetGlowIntensity()
	{
		return flamesGlowIntensity;
	}

	protected override void Awake()
	{
		base.Awake();

		lit = goalSpellID == SpellID.ExtinguishFire;
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

		flamesBlack.color = Util.SetAlpha(flamesBlack.color, lit ? 1 : 0);
		flamesColor.color = Util.SetAlpha(flamesColor.color, flamesGlowIntensity);
		flamesGlow.color = Util.SetAlpha(flamesGlow.color, flamesGlowIntensity);
	}
}
