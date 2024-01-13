using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Statum
{
	public bool value;
	public float time;
	public float TimeSince => Time.time - time;

	public Statum() : this(false) { }

	public Statum(bool value)
	{
		this.value = value;
		time = Time.time;
	}

	public static implicit operator Statum(bool value) => new Statum(value);
	public static implicit operator bool(Statum value) => value.value;
}

