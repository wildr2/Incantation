using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomLighting : Singleton<RoomLighting>
{
	public Color darkTint;
	[HideInInspector]
	public FireProp fireProp;
	[HideInInspector]
	public LampProp lampProp;

	public float Brightness { get; private set; }

	private void Awake()
	{
		fireProp = FindObjectOfType<FireProp>();
		lampProp = FindObjectOfType<LampProp>();
	}

	private void Update()
	{
		Brightness = (lampProp.on ? 1.0f : fireProp.lit ? 0.2f : 0.0f);
	}
}
