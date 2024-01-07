using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TargetType = BookProp;

public class BookProp : Prop
{
	public Statum open;

	public SpriteRenderer sprite; 
	public Text bookText;
	public AudioClip openSFX;
	public AudioClip closeSFX;

	private List<Spell> pages = new List<Spell>();
	private int pageIndex;


	public void Toggle()
	{
		if (open)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	public void Open()
	{
		if (!open && pages.Count > 0)
		{
			open = true;
			SFXManager.Play(openSFX, MixerGroup.Book, transform.position);

			// Open to the spell the current card teaches.
			Card card = GameManager.Instance.CurrentCard;
			if (card)
			{
				pageIndex = pages.FindIndex(p => p.SpellID == card.goalSpellID);
			}
		}
	}

	public void Close()
	{
		if (open)
		{
			open = false;
			SFXManager.Play(closeSFX, MixerGroup.Book, transform.position);
		}
	}

	public void TurnPage(int dir)
	{
		if (open)
		{
			pageIndex = Mathf.Clamp(pageIndex + dir, 0, pages.Count - 1);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		open = false;
	}

	protected override void Update()
	{
		base.Update();
		sprite.gameObject.SetActive(open);
		UpdateAddPages();
		UpdateText();
	}

	private void UpdateAddPages()
	{
		Card card = GameManager.Instance.CurrentCard;
		if (!card)
		{
			return;
		}

		Spell spell = Player.Instance.spells[card.goalSpellID];
		if (!spell.seen)
		{
			//Spell currentPageSpell = pages.Count > 0 ? pages[pageIndex] : null;
			pages.Add(spell);
			pages.Sort(new Spell.NameComparer());
			//if (currentPageSpell != null)
			//{
			//	// Correct page index.
			//	pageIndex = pages.FindIndex(p => p.SpellID == currentPageSpell.SpellID);
			//}

			pageIndex = pages.FindIndex(p => p.SpellID == spell.SpellID);

			Open();
			spell.seen = true;
		}
	}

	private void UpdateText()
	{
		if (pages.Count == 0)
		{
			bookText.text = "";
		}
		else
		{
			Spell pageSpell = pages[pageIndex];

			bookText.text = string.Format("{0}\n\n", pageSpell.SpellID.ToString());
			foreach (IncantationRule rule in pageSpell.incantationDef.rules)
			{
				bookText.text += string.Format("{0}\n", rule.GetDescription());
			}
		}
	}

	[System.Serializable]
	public class ActivateSE : PropSE
	{
		public override SpellID SpellID => SpellID.Activate;
		public new TargetType Target => (TargetType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.open;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			Target.Open();
		}
	}
	public ActivateSE activateSE;

	[System.Serializable]
	public class UnlockSE : PropSE
	{
		public override SpellID SpellID => SpellID.Unlock;
		public UnlockSpell Spell => (UnlockSpell)spellCast.spell;
		public new TargetType Target => (TargetType)base.Target;

		public override bool AreConditionsMet()
		{
			return !Target.open;
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			DoDelayed(Spell.openDelay, Target.Open);
		}
	}
	public UnlockSE unlockSE;
}
