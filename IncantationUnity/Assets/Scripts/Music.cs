using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : Singleton<Music>
{
	private AudioSource source;
	public bool tmpMute;

	public void SetEnabled(bool enabled)
	{
		if (enabled)
		{
			source.Play();
			source.volume = 1;
			tmpMute = false;
		}
		else
		{
			source.Stop();
			source.volume = 0;
		}
	}

	private void Awake()
	{
		source = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (tmpMute)
		{
			source.volume = Mathf.Lerp(source.volume, 0.0f, Time.deltaTime * 2.0f);
		}
	}
}
