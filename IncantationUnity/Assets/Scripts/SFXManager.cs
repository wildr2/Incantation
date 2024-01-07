using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum MixerGroup
{
	Master,
	Ambient,
	Book,
	Magic,
	Music,
}

public class SFXManager : Singleton<SFXManager>
{
	public AudioSource worldSFXSource;
	public AudioMixerGroup[] mixerGroups;
	public MixerGroup[] mixerGroupEnumValues;
	public Dictionary<MixerGroup, AudioMixerGroup> mixerGroupMap;

	public static AudioSource Play(AudioClip clip, MixerGroup mixerGroup = MixerGroup.Master, Vector3? position=null, float pitchOffset=0.0f, float pitchVariance=0.0f, float volume=1.0f, float delay=0.0f)
	{
		if (!clip)
		{
			return null;
		}
		SFXManager instance = Instance;
		AudioSource source = Instantiate(instance.worldSFXSource, instance.transform);
		source.clip = clip;
		source.transform.position = position != null ? (Vector3)position : Vector3.zero;
		source.spatialBlend = position == null ? 0 : 1;
		source.pitch += pitchOffset + Random.Range(-pitchVariance, pitchVariance);
		source.volume = volume;
		source.outputAudioMixerGroup = instance.mixerGroupMap[mixerGroup];
		source.PlayDelayed(delay);
		instance.StartCoroutine(instance.DestroyOnDone(source));
		return source;
	}

	public static AudioSource Play(AudioClip[] clip_variations, MixerGroup mixerGroup = MixerGroup.Master, Vector3? position=null, float pitchOffset=0.0f, float pitchVariance=0.0f, float volume=1.0f, float delay=0.0f)
	{
		AudioClip clip = clip_variations[Random.Range(0, clip_variations.Length)];
		return Play(clip, mixerGroup, position, pitchOffset, pitchVariance, volume, delay);
	}

	private void Awake()
	{
		mixerGroupMap = new Dictionary<MixerGroup, AudioMixerGroup>();
		for (int i = 0; i < mixerGroups.Length; ++i)
		{
			mixerGroupMap[mixerGroupEnumValues[i]] = mixerGroups[i];
		}
	}

	private IEnumerator DestroyOnDone(AudioSource source)
	{
		while (source && source.isPlaying)
		{
			yield return null;
		}
		if (source)
		{
			Destroy(source.gameObject);
		}
	}
}