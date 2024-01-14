using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : Singleton<Music>
{
	private AudioSource source;
	public bool tmpMute;

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
		else
		{
			source.volume = DebugSettings.Instance.disableMusic ? 0.0f : 1.0f;
		}
	}
}
