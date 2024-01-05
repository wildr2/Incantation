using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookProp : Prop
{
	public Text bookText;
	public AudioClip openSound;
	public AudioClip closeSound;

	public bool IsOpen()
	{
		return gameObject.activeInHierarchy;
	}

	public void Toggle()
	{
		if (IsOpen())
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
		if (IsOpen())
		{
			return;
		}
		gameObject.SetActive(true);
		SFXManager.Play(openSound, MixerGroup.Book, transform.position);
	}

	public void Close()
	{
		if (!IsOpen())
		{
			return;
		}
		gameObject.SetActive(false);
		SFXManager.Play(closeSound, MixerGroup.Book, transform.position);
	}

	protected override void Update()
	{
		base.Update();
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
}
