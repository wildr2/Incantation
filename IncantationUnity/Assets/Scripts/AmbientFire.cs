using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientFire : MonoBehaviour
{
	private AudioSource source;

	public bool IsLit()
	{
		return source.volume > 0;
	}

	public void Light()
	{
		source.volume = 1.0f;
	}

	public void Extinguish()
	{
		source.volume = 0.0f;
	}

	private void Awake()
	{
		source = GetComponent<AudioSource>();
	}
}
