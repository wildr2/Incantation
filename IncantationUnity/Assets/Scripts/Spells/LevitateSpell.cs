using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevitateSpell : Spell
{
	public AudioClip[] castSFXIntro;

	public LevitateSpell() : base(SpellID.Levitate)
	{
	}
}
