using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : Singleton<SFXManager>
{
	public AudioSource worldSFXSource;

	public static void Play(AudioClip clip, Vector3? position=null, float pitchOffset=0.0f, float pitchVariance=0.0f, float volume=1.0f, float delay=0.0f)
	{
		SFXManager instance = Instance;
		AudioSource source = Instantiate(instance.worldSFXSource, instance.transform);
		source.clip = clip;
		source.transform.position = position != null ? (Vector3)position : Vector3.zero;
		source.pitch += pitchOffset + Random.Range(-pitchVariance, pitchVariance);
		source.volume = volume;
		source.PlayDelayed(delay);
		instance.StartCoroutine(instance.DestroyOnDone(source));
	}

	private IEnumerator DestroyOnDone(AudioSource source)
	{
		while (source.isPlaying)
		{
			yield return null;
		}
		Destroy(source.gameObject);
	}
}