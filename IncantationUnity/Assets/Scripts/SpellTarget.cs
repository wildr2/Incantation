using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTarget : MonoBehaviour
{
	public virtual SpellEffect[] SpellEffects => GetComponents<SpellEffect>();
}
