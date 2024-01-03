using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FirePropCreateFireSE))]
[RequireComponent(typeof(FirePropExtinguishFireSE))]
public class FireProp : Prop
{
	public Statum lit;
	private AudioSource fireSFXSource;

	private void Awake()
	{
		fireSFXSource = GetComponent<AudioSource>();
		lit = true;
	}

	private void Update()
	{
		fireSFXSource.volume = lit ? 1.0f : 0.0f;
	}
}
