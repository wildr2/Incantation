using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class Player : Singleton<Player>
{
	public Spell genericSpell = new Spell(SpellID.Generic);
	public Spell createFireSpell = new Spell(SpellID.CreateFire);
	public Spell extinguishFireSpell = new Spell(SpellID.ExtinguishFire);
	public Spell explodeSpell = new Spell(SpellID.Explode);
	public Spell vanishSpell = new Spell(SpellID.Vanish);
	public Spell rainSpell = new Spell(SpellID.Rain);
	public Spell levitate = new Spell(SpellID.Levitate);
	public Spell activate = new Spell(SpellID.Activate);
	public Spell deactivate = new Spell(SpellID.Deactivate);
	public Spell breakSpell = new Spell(SpellID.Break);
	public Spell mend = new Spell(SpellID.Mend);
	public Dictionary<SpellID, Spell> spells;

	private Book book;

	private void Awake()
	{
		book = FindObjectOfType<Book>();

		// Create dictionary of spells using reflection.
		spells = new Dictionary<SpellID, Spell>();
		System.Type playerType = typeof(Player);
		foreach (FieldInfo field in playerType.GetFields(BindingFlags.Public | BindingFlags.Instance))
		{
			if (field.FieldType == typeof(Spell))
			{
				Spell spell = (Spell)field.GetValue(this);
				spells[spell.spellID] = spell;
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
