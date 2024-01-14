using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircumstancesChecker : Singleton<CircumstancesChecker>
{
	private RoomLighting roomLighting;
	private RainProp rainProp;

	public IncantationCircumstance GetCircumstances()
	{
		Card card = GameManager.Instance.CurrentCard;

		IncantationCircumstance c = 0;
		c |= roomLighting.Brightness < 0.5f ? IncantationCircumstance.Dark : 0;
		c |= roomLighting.Brightness <= 0.0f ? IncantationCircumstance.PitchBlack : 0;

		// XXX: Needs to consider raining in cards to prevent case where you can't cast the goal spell
		//      because it requires raining, but the rain spell affects the card only.
		c |= rainProp.raining || card != null && card.Raining ? IncantationCircumstance.Raining : 0;

		return c;
	}


	private void Awake()
	{
		roomLighting = RoomLighting.Instance;
		rainProp = FindObjectOfType<RainProp>();
	}
}
