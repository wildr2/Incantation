using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTarget : MonoBehaviour
{
	public int priority;
	public virtual SpellEffect[] SpellEffects => GetComponents<SpellEffect>();

	// Descending priority.
	public class ComparerPriority : IComparer<SpellTarget>
	{
		public int Compare(SpellTarget a, SpellTarget b)
		{
			return b.priority.CompareTo(a.priority);
		}
	}
}
