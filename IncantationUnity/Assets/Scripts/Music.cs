using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
	private AudioSource source;
	public bool tmpMute;

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
				source.volume = 1.0f;
				tmpMute = false;
			}
		}

		if (tmpMute)
		{
			source.volume = Mathf.Lerp(source.volume, 0.0f, Time.deltaTime * 2.0f);
		}
	}
}
