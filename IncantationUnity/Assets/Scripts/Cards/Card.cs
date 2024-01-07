using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using CardType = Card;

[RequireComponent(typeof(CommonCardData))]
public class Card : SpellTarget
{
	public override int Priority => 1;
	public SpellID goalSpellID;
	[HideInInspector]
	public SpellID lastSpellID;
	private Vector3 initialPos;
	private CommonCardData commonCardData;

	public virtual bool IsComplete()
	{
		return false;
	}

	public virtual bool IsSkippable()
	{
		return false;
	}

	public virtual float GetGlowIntensity()
	{
		return 0.0f;
	}

	public void Shake(float intensity)
	{
		float maxAngle = 8;
		float maxRotation = 3;
		float rotateAngle = maxRotation * intensity * (Random.value > 0.5f ? -1 : 1);
		if (Mathf.Abs(transform.rotation.eulerAngles.z + rotateAngle) > maxAngle)
		{
			rotateAngle *= -1;
		}
		transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + rotateAngle);

		float maxOffset = 1.0f;
		float dirAngle = Random.Range(0, Mathf.PI);
		Vector3 dir = new Vector3(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle));
		Vector3 translation = dir * intensity * 0.2f;
		if (Vector3.Distance(transform.position + translation, initialPos) > maxOffset)
		{
			translation = (initialPos - transform.position) * translation.magnitude;
		}
		transform.position += translation;
	}

	protected override void Awake()
	{
		base.Awake();
		initialPos = transform.position;
		commonCardData = GetComponent<CommonCardData>();
	}

	protected override void Update()
	{
		base.Update();
		UpdateDebugText();
	}

	private void UpdateDebugText()
	{
		bool enable = DebugSettings.Instance.enableCardDebugText;
		commonCardData.debugText.enabled = enable;
		if (!enable)
		{
			return;
		}

		string text = "";

		// Statums.
		System.Type cardType = GetType();
		foreach (FieldInfo field in cardType.GetFields(BindingFlags.Public | BindingFlags.Instance))
		{
			if (field.FieldType == typeof(Statum))
			{
				Statum statum = (Statum)field.GetValue(this);
				if (statum == null)
				{
					Debug.LogError(string.Format("Statum '{0}' of card '{1}' not initialized.", field.Name, transform.name));
					continue;
				}
				text += string.Format("{2}{0}: {1}", field.Name, statum.value, text.Length > 0 ? "\n" : "");
			}
		}

		// Completion state.
		text += "\n";
		text += string.Format("\ncomplete: {0}", IsComplete());
		text += string.Format("\nskippable: {0}", IsSkippable());

		commonCardData.debugText.text = text;
	}

	[System.Serializable]
	public abstract class CardSE : SpellEffect
	{
		public new CardType Target => (CardType)base.Target;
		public CommonCardData CardData => Target.commonCardData;
		public virtual bool ShakeOnApply => true;

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.lastSpellID = SpellID;
			if (ShakeOnApply)
			{
				Shake(spellCast.intensity);
			}
		}

		protected virtual void Shake(float intensity)
		{
			Target.Shake(Util.Map(0, 1, 0.5f, 1.0f, intensity));
		}
	}

	[System.Serializable]
	public class GenericSE : CardSE
	{
		public override SpellID SpellID => SpellID.Generic;

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			SFXManager.Play(CardData.genericSpellSFX, MixerGroup.Magic);
		}

		protected override void Shake(float intensity)
		{
			// Shake the card only a small amount.
			Target.Shake(Util.Map(0, 1, 0.2f, 0.3f, intensity));
		}
	}
	public GenericSE genericSE;

	[System.Serializable]
	public abstract class LevitateSE : CardSE
	{
		public override SpellID SpellID => SpellID.Levitate;
		protected abstract Statum Levitating { get; set; }
		public AudioClip landSFX;
		protected bool visuallyLevitating;

		public override bool AreConditionsMet()
		{
			return !Levitating;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			StartLevitating();
		}

		public override void Update()
		{
			base.Update();
			if (visuallyLevitating)
			{
				if (!Levitating || spellCast.CastTime >= spellCast.spell.effectEndTime)
				{
					// Interrupted or duration up.
					EndLevitation();
				}
			}
		}

		protected virtual void StartLevitating()
		{
			Levitating = true;
			visuallyLevitating = true;
			SetContentPosition(true);
		}

		protected virtual void EndLevitation()
		{
			Levitating = false;
			visuallyLevitating = false;
			SetContentPosition(false);

			if (spellCast.audioSource)
			{
				spellCast.audioSource.Stop();
			}

			if (CanLandLevitation())
			{
				SFXManager.Play(landSFX, MixerGroup.Master);
			}
		}

		protected virtual bool CanLandLevitation()
		{
			return true;
		}

		protected virtual void SetContentPosition(bool levitated)
		{
			CardData.contentParent.transform.localPosition = levitated ? CardData.levitatePos : Vector2.zero;
		}
	}

	[System.Serializable]
	public abstract class RainSE : CardSE
	{
		public override SpellID SpellID => SpellID.Rain;
		protected abstract Statum Raining { get; set; }
		protected virtual AudioClip[] AudioClips => new AudioClip[] { CardData.rainingSFX };
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
					Destroy(sources[i].gameObject);
					sources.RemoveAt(i);
				}
			}
			foreach (AudioClip clip in clips)
			{
				AudioSource source = sources.Find(s => s.clip == clip);
				if (!source)
				{
					source = SFXManager.Play(clip, loop: true);
					sources.Add(source);
				}
				source.volume = volume;
			}
		}
	}
}
