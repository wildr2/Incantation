using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnlockSpell : Spell
{
	public float openDelay;

	public UnlockSpell() : base(SpellID.Unlock)
	{
	}
}
