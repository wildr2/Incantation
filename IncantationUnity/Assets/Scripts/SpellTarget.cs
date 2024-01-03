using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class SpellTarget : MonoBehaviour
{
	public virtual int Priority => 0;
	public SpellEffect[] SpellEffects { get; private set; }

	protected virtual void Awake()
	{
		// Populate SpellEffects using reflection.
		List<SpellEffect> effects = new List<SpellEffect>();
		System.Type targetType = GetType();
		foreach (FieldInfo field in targetType.GetFields(BindingFlags.Public | BindingFlags.Instance))
		{
			SpellEffect effect = field.GetValue(this) as SpellEffect;
			if (effect != null)
			{
				effect.Init(this);
				effects.Add(effect);
			}
		}
		SpellEffects = effects.ToArray();
	}

	// Descending priority.
	public class PriorityComparer : IComparer<SpellTarget>
	{
		public int Compare(SpellTarget a, SpellTarget b)
		{
			return b.Priority.CompareTo(a.Priority);
		}
	}
}
