using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class BaseRainSE : SpellEffect
{
	public override SpellID SpellID => SpellID.Rain;
	protected abstract Statum Raining { get; set; }
	protected virtual AudioClip[] AudioClips => new AudioClip[] { };
	protected float volume;
	protected List<AudioSource> sources = new List<AudioSource>();
	private const float rainingDuration = 5.0f;

	public override void Apply(SpellCast spellCast)
	{
		base.Apply(spellCast);
		Raining = true;
	}

	public override void Update()
	{
		base.Update();

		if (Raining && Time.time - Raining.time > rainingDuration)
		{
			Raining = false;
		}

		volume = Mathf.Lerp(volume, Raining ? 1 : 0, Time.deltaTime);
		if (!Raining && volume < 0.01f)
		{
			volume = 0.0f;
		}
		UpdateAudioSources(volume);
	}

	private void UpdateAudioSources(float volume)
	{
		if (volume <= 0 && sources.Count == 0)
		{
			return;
		}
		AudioClip[] clips = AudioClips;
		for (int i = sources.Count - 1; i >= 0; --i)
		{
			if (!System.Array.Find(clips, c => c == sources[i].clip))
			{
				Object.Destroy(sources[i].gameObject);
				sources.RemoveAt(i);
			}
		}
		foreach (AudioClip clip in clips)
		{
			AudioSource source = sources.Find(s => s.clip == clip);
			if (!source)
			{
				source = SFXManager.Play(clip, loop: true, parent: Target.transform);
				sources.Add(source);
			}
			source.volume = volume;
		}
	}
}

[System.Serializable]
public abstract class CardRainSE : BaseRainSE
{
	protected override AudioClip[] AudioClips => new AudioClip[] { CommonCardData.rainingSFX };
}
