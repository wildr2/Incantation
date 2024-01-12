using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class Player : Singleton<Player>
{
	public Spell activateSpell = new Spell(SpellID.Activate);
	public Spell breakSpell = new Spell(SpellID.Break);
	public Spell deactivateSpell = new Spell(SpellID.Deactivate);
	public Spell explodeSpell = new Spell(SpellID.Explode);
	public Spell extinguishSpell = new Spell(SpellID.Extinguish);
	public Spell fillSpell = new Spell(SpellID.Fill);
	public Spell genericSpell = new Spell(SpellID.Generic);
	public Spell growSpell = new Spell(SpellID.Grow);
	public Spell igniteSpell = new Spell(SpellID.Ignite);
	public LevitateSpell levitateSpell = new LevitateSpell();
	public Spell lockSpell = new Spell(SpellID.Lock);
	public Spell mendSpell = new Spell(SpellID.Mend);
	public Spell rainSpell = new Spell(SpellID.Rain);
	public Spell summonStendariiSpell = new Spell(SpellID.SummonStendarii);
	public UnlockSpell unlockSpell = new UnlockSpell();
	public Spell vanishSpell = new Spell(SpellID.Vanish);

	public Dictionary<SpellID, Spell> spells;
	private BookProp book;
	public System.Action OnOpenedBook;
	public System.Action OnTurnedPage;

	private void Awake()
	{
		book = FindObjectOfType<BookProp>();

		// Init spells, and create dictionary of spells using reflection.
		List<IncantationDef> incantationDefs = new List<IncantationDef>();
		spells = new Dictionary<SpellID, Spell>();
		System.Type playerType = typeof(Player);
		foreach (FieldInfo field in playerType.GetFields(BindingFlags.Public | BindingFlags.Instance))
		{
			Spell spell = field.GetValue(this) as Spell;
			if (spell != null)
			{
				IncantationDef incantationDef = IncantationDef.CreateUnique(spell.incantationDefConfig, incantationDefs);
				incantationDefs.Add(incantationDef);
				spell.Init(incantationDef);
				spells[spell.SpellID] = spell;
			}
		}
	}

	private void Update()
	{
		bool input_book = Input.GetKeyDown(KeyCode.Tab);
		bool input_book_page_left = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Alpha1);
		bool input_book_page_right = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Alpha2);
		int input_book_page_dir = input_book_page_left ? -1 : input_book_page_right ? 1 : 0;

		if (input_book)
		{
			book.Toggle();
			if (book.open && OnOpenedBook != null)
			{
				OnOpenedBook();
			}
		}
		if (input_book_page_dir != 0)
		{
			if (book.TurnPage(input_book_page_dir) && OnTurnedPage != null)
			{
				OnTurnedPage();
			}
		}
	}
}
