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

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Target.lastSpellID = SpellID;
			Shake(intensity);
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

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
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

		private const float levitateDuration = 4.0f;
		private bool active;

		public override bool AreConditionsMet()
		{
			return !Levitating;
		}

		public override void Apply(float intensity)
		{
			base.Apply(intensity);
			Levitating = true;
			active = true;
			SetContentPosition(true);
		}

		public override void Update()
		{
			base.Update();
			if (active)
			{
				if (!Levitating || Time.time - Levitating.time > levitateDuration)
				{
					EndLevitation();
				}
			}
		}

		protected virtual void EndLevitation()
		{
			Levitating = false;
			active = false;
			SetContentPosition(false);
		}

		protected virtual void SetContentPosition(bool levitated)
		{
			CardData.contentParent.transform.localPosition = levitated ? CardData.levitatePos : Vector2.zero;
		}
	}
}
