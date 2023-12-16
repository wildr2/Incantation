using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
	private AudioSource source;

	private void Awake()
	{
		source = GetComponent<AudioSource>();
	}

	private void Update()
	{
		bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		if (shift && Input.GetKeyDown(KeyCode.M))
		{
			if (source.isPlaying)
			{
				source.Stop();
			}
			else
			{
				source.Play();
			}
		}
	}
}
