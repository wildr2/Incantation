using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircumstancesChecker : Singleton<CircumstancesChecker>
{
	private RoomLighting roomLighting;
	private RainProp rainProp;

	public IncantationCircumstance GetCircumstances()
	{
		IncantationCircumstance c = 0;
		c |= roomLighting.Brightness < 0.5f ? IncantationCircumstance.Dark : 0;
		c |= roomLighting.Brightness <= 0.0f ? IncantationCircumstance.PitchBlack : 0;
		c |= rainProp.raining ? IncantationCircumstance.Raining : 0;
		return c;
	}

	private void Awake()
	{
		roomLighting = RoomLighting.Instance;
		rainProp = FindObjectOfType<RainProp>();
	}
}
