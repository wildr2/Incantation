using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomLighting : Singleton<RoomLighting>
{
	[HideInInspector]
	public FireProp fireProp;
	[HideInInspector]
	public LampProp lampProp;

	public float Brightness { get; private set; }
	private Color tint = Color.white;
	private bool tmpFlashingRed;

	public Color TintColor(Color color)
	{
		return Util.LerpColorHSV(color, tint, 0.5f);
	}

	public void TmpFlashRed()
	{
		tmpFlashingRed = true;
		Brightness = 0.2f;
		tint = Color.blue;
	}

	private void Awake()
	{
		fireProp = FindObjectOfType<FireProp>();
		lampProp = FindObjectOfType<LampProp>();
	}

	private void Update()
	{
		if (tmpFlashingRed)
		{
			Brightness = Mathf.Lerp(Brightness, 0.0f, Time.deltaTime * 1.0f);
			if (lampProp.on || fireProp.lit)
			{
				tmpFlashingRed = false;
				tint = Color.white;
			}
		}
		if (!tmpFlashingRed)
		{
			Brightness = (lampProp.on ? 1.0f : fireProp.lit ? 0.2f : 0.0f);
		}
	}
}
