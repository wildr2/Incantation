using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CardSE))]
public class Card : SpellTarget
{
	public SpellID goalSpellID;
	public Text debugText;
	[HideInInspector]
	public SpellID lastSpellID;
	private Vector3 initialPos;

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

	protected virtual void Awake()
	{
		initialPos = transform.position;
	}

	protected virtual void Update()
	{
		// display card state
	}
}

public abstract class CardSE : SpellEffect
{
	public new Card Target => Target;

	public override bool AreConditionsMet()
	{
		return true;
	}

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
