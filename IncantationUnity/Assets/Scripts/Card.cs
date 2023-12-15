using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
	public SpriteRenderer flamesBlack;
	public SpriteRenderer flamesColor;
	public SpriteRenderer flamesGlow;
	public float lightFireTime = -1;
	public float extinguishFireTime = -1;
	public float glow;
	public float glowDuration;
	
	public void LightFire(float intensity)
	{
		lightFireTime = Time.time;
		glowDuration = Mathf.Lerp(3.0f, 9.0f, intensity);
	}

	public void ExtinguishFire()
	{
		extinguishFireTime = Time.time;
	}

	private void Update()
	{
		bool fireLit = lightFireTime > extinguishFireTime;
		if (fireLit)
		{
			float flicker = Mathf.Lerp(1.0f, Mathf.PerlinNoise(Time.time * 8.0f, 0), 0.2f);
			float fade = Mathf.Lerp(1.0f, 0.0f, (Time.time - lightFireTime) / glowDuration);
			fade = 1.0f - Mathf.Pow(1.0f - fade, 4.0f);
			glow = fade * flicker;
		}
		else
		{
			glow = 0;
		}

		flamesBlack.color = Util.SetAlpha(flamesBlack.color, fireLit ? 1 : 0);
		flamesColor.color = Util.SetAlpha(flamesColor.color, glow);
		flamesGlow.color = Util.SetAlpha(flamesGlow.color, glow);
	}
}
