using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

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

	// Indexed by SpellID.
	public Spell[] GetSpellsArray()
	{
		int n = Util.GetEnumCount<SpellID>();
		Spell[] arr = new Spell[n];
		foreach (Spell spell in spells.Values)
		{
			arr[(int)spell.SpellID] = spell;
		}
		return arr;
	}

	public IEnumerable<Spell> GetLearnedSpells()
	{
		return spells.Values.Where(s => s.seen);
	}

	private void Awake()
	{
		book = FindObjectOfType<BookProp>();

		// Create dictionary of spells using reflection.
		spells = new Dictionary<SpellID, Spell>();
		System.Type playerType = typeof(Player);
		foreach (FieldInfo field in playerType.GetFields(BindingFlags.Public | BindingFlags.Instance))
		{
			Spell spell = field.GetValue(this) as Spell;
			if (spell != null)
			{
				spells[spell.SpellID] = spell;
			}
		}

		// Create incantation defs and init spells.
		Spell[] spellArray = spells.Values.ToArray();
		Util.Shuffle(spellArray);
		IncantationDefConfig[] incantationDefConfigs = spellArray.Select(s => s.incantationDefConfig).ToArray();
		SpellID[] spellIDArray = spellArray.Select(s => s.SpellID).ToArray();
		IncantationDef[] incantationDefs = IncantationDef.CreateUniqueDefs(spellIDArray, incantationDefConfigs).ToArray();
		for (int i = 0; i < spells.Values.Count; ++i)
		{
			spellArray[i].Init(incantationDefs[i]);
		}

		if (DebugSettings.Instance.debugIncantationDefs)
		{
			string str = "INCANTATION DEF DEBUG:\n";
			foreach (Spell spell in spells.Values)
			{
				str += string.Format("{0}\n---------\n", spell.GetDescription());
			}
			Debug.Log(str);
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
