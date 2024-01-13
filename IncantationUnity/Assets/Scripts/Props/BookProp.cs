using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TargetType = BookProp;

public class BookProp : Prop
{
	public Statum open;

	public Text bookText;
	public AudioClip openSFX;
	public AudioClip quickOpenSFX;
	public AudioClip closeSFX;
	public AudioClip[] turnPageSFX;
	public SpriteRenderer spriteRenderer;
	public Sprite litSprite;
	public Sprite unlitSprite;
	public float newSpellOpenDelay = 0.2f;
	public float newSpellOpenDisplayDelay = 0.1f;
	public float openDisplayDelay = 1.0f;
	public float turnDisplayDelay = 0.2f;

	private List<Spell> pages = new List<Spell>();
	private int pageIndex;
	private float pageTurnTime;
	private bool doingQuickOpen;
	private AudioSource openAudioSource;

	public bool DisplayingPage => bookText.text.Length > 0;
	public int PageCount => pages.Count;

	public bool CanOpen()
	{
		return !open && PageCount > 0;
	}

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

	public void Open(bool quick=false)
	{
		if (!open && pages.Count > 0)
		{
			open = true;
			doingQuickOpen = quick;
			openAudioSource = SFXManager.Play(quick ? quickOpenSFX : openSFX, MixerGroup.Book, transform.position);

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
			if (openAudioSource)
			{
				openAudioSource.Stop();
			}
		}
	}

	public bool TurnPage(int dir)
	{
		if (open)
		{
			int oldPageIndex = pageIndex;
			pageIndex = Mathf.Clamp(pageIndex + dir, 0, pages.Count - 1);
			if (pageIndex != oldPageIndex)
			{
				pageTurnTime = Time.time;
				SFXManager.Play(turnPageSFX, MixerGroup.Book);
				return true;
			}
		}
		return false;
	}

	public bool HasPage(SpellID spellID)
	{
		return pages.FindIndex(p => p.SpellID == spellID) >= 0;
	}

	public void AddPage(SpellID spellID, bool openToPage=false)
	{
		Spell spell = Player.Instance.spells[spellID];
		pages.Add(spell);
		pages.Sort(new Spell.NameComparer());

		pageIndex = pages.FindIndex(p => p.SpellID == spellID);

		if (openToPage)
		{
			StartCoroutine(CoroutineUtil.DoAfterDelay(() =>
			{
				Open(quick: true);
			}, newSpellOpenDelay));
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
		UpdateAddPages();
		UpdateText();
		UpdateSprite();
	}

	private void UpdateSprite()
	{
		spriteRenderer.gameObject.SetActive(open);
		if (open)
		{
			RoomLighting rl = RoomLighting.Instance;
			spriteRenderer.sprite = rl.lampProp.on ? litSprite : unlitSprite;
		}
	}

	private void UpdateAddPages()
	{
		Card card = GameManager.Instance.CurrentCard;
		if (!card)
		{
			return;
		}

		Spell spell = Player.Instance.spells[card.goalSpellID];
		if (!spell.seen && !HasPage(spell.SpellID))
		{
			AddPage(spell.SpellID, openToPage: PageCount != 0);
		}
	}

	private void UpdateText()
	{
		float openDisplayDelay = doingQuickOpen ? newSpellOpenDisplayDelay : this.openDisplayDelay;
		bool delaying = 
			Time.time - open.time < openDisplayDelay ||
			Time.time - pageTurnTime < turnDisplayDelay;

		if (!open || delaying || pages.Count == 0)
		{
			bookText.text = "";
		}
		else
		{
			// Display page.
			Spell pageSpell = pages[pageIndex];
			pageSpell.seen = true;

			bookText.text = pageSpell.GetDescription();
		}
	}

	[System.Serializable]
	public class ActivateSE : PropSE
	{
		public override SpellID SpellID => SpellID.Activate;
		public new TargetType Target => (TargetType)base.Target;

		public override bool AreConditionsMet()
		{
			return Target.CanOpen();
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
			return Target.CanOpen();
		}

		public override void Apply(SpellCast spellCast)
		{
			base.Apply(spellCast);
			DoDelayed(Spell.openDelay, () => Target.Open());
		}
	}
	public UnlockSE unlockSE;
}
