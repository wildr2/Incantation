using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FirePropCreateFireSE))]
[RequireComponent(typeof(FirePropExtinguishFireSE))]
public class FireProp : Prop
{
	public Statum lit;
	private AudioSource fireSFX;

	private void Awake()
	{
		fireSFX = GetComponent<AudioSource>();
	}

	private void Update()
	{
		fireSFX.volume = lit ? 1.0f : 0.0f;
	}
}
