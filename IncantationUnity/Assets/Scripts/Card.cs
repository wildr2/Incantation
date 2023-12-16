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
	private Vector3 initialPos;
	
	public bool IsFireLit()
	{
		return lightFireTime > extinguishFireTime;
	}

	public void LightFire(float intensity)
	{
		lightFireTime = Time.time;
		glowDuration = Mathf.Lerp(3.0f, 9.0f, intensity);
	}

	public void ExtinguishFire()
	{
		extinguishFireTime = Time.time;
	}

	public void Shake(float intensity)
	{
		float maxAngle = 8;
		float maxRotation = 3;
		float rotateAngle = maxRotation * intensity * (Random.value > 0.5f ? -1 : 1);
		if (Mathf.Abs(transform.rotation.eulerAngles.z + rotateAngle) > maxAngle)
		{
			rotateAngle *= -1;
		}
		transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + rotateAngle);

		float maxOffset = 1.0f;
		float dirAngle = Random.Range(0, Mathf.PI);
		Vector3 dir = new Vector3(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle));
		Vector3 translation = dir * intensity * 0.2f;
		if (Vector3.Distance(transform.position + translation, initialPos) > maxOffset)
		{
			translation = (initialPos - transform.position) * translation.magnitude;
		}
		transform.position += translation;
	}

	private void Awake()
	{
		initialPos = transform.position;
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
