using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;

[RequireComponent(typeof(CommonCardData))]
public class Card : SpellTarget
{
	public override float Priority => 1;
	public SpellID goalSpellID;
	private Vector3 initialPos;
	[HideInInspector]
	public CommonCardData commonCardData;
	private List<CardGlow> glows = new List<CardGlow>();

	public virtual bool IsComplete()
	{
		return false;
	}

	public virtual bool IsSkippable()
	{
		return false;
	}

	public float GetGlowIntensity()
	{
		return glows.Count > 0 ? glows.Max(x => x.Intensity) : 0;
	}

	public Color GetGlowColor()
	{
		Color c = Color.clear;
		foreach (CardGlow glow in glows)
		{
			Color c1 = glow.Color;
			c = new Color(c.r + c1.r, c.g + c1.g, c.b + c1.b, c.a + c1.a);
		}
		return c;
	}

	public void Glow(SpriteRenderer spriteRenderer)
	{
		glows.Add(new CardGlow(spriteRenderer, 2.0f));
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
		UpdateGlows();
		UpdateDebugText();
	}

	private void UpdateGlows()
	{
		for (int i = glows.Count - 1; i >= 0; --i)
		{
			if (glows[i].Update())
			{
				glows[i].End();
				glows.RemoveAt(i);
			}
		}
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
	public class GenericSE : SpellEffect
	{
		public override SpellID SpellID => SpellID.Generic;

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			SFXManager.Play(CommonCardData.genericSpellSFX, MixerGroup.Magic, parent: Target.transform);
		}

		protected override void ShakeCard(Card card, float intensity)
		{
			// Shake the card only a small amount.
			card.Shake(Util.Map(0, 1, 0.2f, 0.3f, intensity));
		}
	}
	public GenericSE genericSE;
}

public class CardGlow
{
	public float Intensity => spriteRenderer.color.a;
	public Color Color => spriteRenderer.color;
	private SpriteRenderer spriteRenderer;
	private float startTime;
	private float duration;

	public CardGlow(SpriteRenderer spriteRenderer, float duration)
	{
		this.spriteRenderer = spriteRenderer;
		startTime = Time.time;
		this.duration = duration;

		spriteRenderer.enabled = true;
	}

	public bool Update()
	{
		float flicker = Mathf.Lerp(1.0f, Mathf.PerlinNoise(Time.time * 8.0f, 0), 0.1f);
		float fade = Mathf.Lerp(1.0f, 0.0f, (Time.time - startTime) / duration);
		fade = 1.0f - Mathf.Pow(1.0f - fade, 4.0f);
		float glow = fade * flicker;
		spriteRenderer.color = Util.SetAlpha(spriteRenderer.color, glow);
		return glow <= 0.0f;
	}

	public void End()
	{
		spriteRenderer.enabled = false;
	}
}
