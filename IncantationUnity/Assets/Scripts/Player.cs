using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class Player : Singleton<Player>
{
	public Spell activateSpell = new Spell(SpellID.Activate);
	public Spell breakSpell = new Spell(SpellID.Break);
	public Spell createFireSpell = new Spell(SpellID.Ignite);
	public Spell deactivateSpell = new Spell(SpellID.Deactivate);
	public Spell explodeSpell = new Spell(SpellID.Explode);
	public Spell extinguishFireSpell = new Spell(SpellID.Extinguish);
	public Spell genericSpell = new Spell(SpellID.Generic);
	public Spell growSpell = new Spell(SpellID.Grow);
	public Spell levitateSpell = new Spell(SpellID.Levitate);
	public Spell lockSpell = new Spell(SpellID.Lock);
	public Spell mendSpell = new Spell(SpellID.Mend);
	public Spell rainSpell = new Spell(SpellID.Rain);
	public Spell refillSpell = new Spell(SpellID.Fill);
	public Spell unlockSpell = new Spell(SpellID.Unlock);
	public Spell vanishSpell = new Spell(SpellID.Vanish);

	public Dictionary<SpellID, Spell> spells;
	private BookProp book;

	private void Awake()
	{
		book = FindObjectOfType<BookProp>();

		// Init spells, and create dictionary of spells using reflection.
		spells = new Dictionary<SpellID, Spell>();
		System.Type playerType = typeof(Player);
		foreach (FieldInfo field in playerType.GetFields(BindingFlags.Public | BindingFlags.Instance))
		{
			if (field.FieldType == typeof(Spell))
			{
				Spell spell = (Spell)field.GetValue(this);
				spell.Init();
				spells[spell.SpellID] = spell;
			}
		}
	}

	private void Update()
	{
		bool book_input = Input.GetKeyDown(KeyCode.Tab);
		if (book_input)
		{
			book.Toggle();
		}
	}
}
