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
		if (!open)
		{
			open = true;
			SFXManager.Play(openSFX, MixerGroup.Book, transform.position);
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

	protected override void Awake()
	{
		base.Awake();
		open = true;
	}

	protected override void Update()
	{
		base.Update();
		sprite.gameObject.SetActive(open);
		UpdateText();
	}

	private void UpdateText()
	{
		GameManager gm = GameManager.Instance;
		Card card = gm.CurrentCard;
		if (!card)
		{
			return;
		}
		Spell spell = Player.Instance.spells[card.goalSpellID];

		bookText.text = string.Format("{0}\n\n", spell.SpellID.ToString());
		foreach (IncantationRule rule in spell.incantationDef.rules)
		{
			bookText.text += string.Format("{0}\n", rule.GetDescription());
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
