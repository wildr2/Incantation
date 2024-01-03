using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampCard : Card
{
	public Statum on;
	public Statum broken;
	public Statum levitating;
	public Statum vanished;

	public SpriteRenderer lampOn;
	public SpriteRenderer lampOff;
	public SpriteRenderer lampBroken;

	protected override void Awake()
	{
		base.Awake();

		on = goalSpellID == SpellID.Activate;
		broken = false;
		levitating = false;
		vanished = false;
	}

	protected override void Update()
	{
		base.Update();
		UpdateLamp();
	}

	private void UpdateLamp()
	{
		lampOn.enabled = on;
		lampOff.enabled = !on;
		lampBroken.enabled = broken;
	}
}
