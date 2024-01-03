using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statum
{
	public bool value;
	public float time;

	public Statum() : this(false) { }

	public Statum(bool value)
	{
		this.value = value;
		time = Time.time;
	}

	public static implicit operator Statum(bool value)
	{
		return new Statum(value);
	}

	public static implicit operator bool(Statum value)
	{
		return value.value;
	}
}

