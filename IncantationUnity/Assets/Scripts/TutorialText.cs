using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialState
{
	Init,
	OpenBook,
	OpenBookDone,
	CastSpell,
	TurnPages,
	Done,
}

public class TutorialText : MonoBehaviour
{
	public string Text { get; private set; }

	private Incantor incantor;
	private Player player;
	private BookProp book;
	private TutorialState state = TutorialState.Init;
	private const float textLag = 0.5f;
	private string pendingText;
	private float pendingTextChangedTime;
	private string PendingText
	{ 
		set
		{
			if (pendingText != value)
			{
				pendingText = value;
				pendingTextChangedTime = Time.time;
			}
		}
		get => pendingText;
	}

	private void Awake()
	{
		player = Player.Instance;
		incantor = FindAnyObjectByType<Incantor>();
		book = FindAnyObjectByType<BookProp>();

		player.OnOpenedBook += () =>
		{
			if (state == TutorialState.OpenBook)
			{
				SetState(TutorialState.OpenBookDone);
			}
		};
		incantor.onCastSpell += () =>
		{
			if (state == TutorialState.CastSpell)
			{
				SetState(TutorialState.TurnPages);
			}
		};
		player.OnTurnedPage += () =>
		{
			if (state == TutorialState.TurnPages)
			{
				SetState(TutorialState.Done);
			}
		};
	}

	private void Update()
	{
		const string tutorialCastSpellText = "type and press enter to cast spells";
		const string tutorialOpenBookText = "press tab to open the spell book";
		const string tutorialTurnPagesText = "press left and right arrow keys to turn pages";

		if (state == TutorialState.Init)
		{
			if (GameManager.Instance.CurrentCard != null)
			{
				SetState(TutorialState.OpenBook);
			}
		}
		else if (state == TutorialState.OpenBook)
		{
			PendingText = tutorialOpenBookText;
		}
		else if (state == TutorialState.OpenBookDone)
		{
			PendingText = "";
			if (book.DisplayingPage)
			{
				SetState(TutorialState.CastSpell);
			}
		}
		else if (state == TutorialState.CastSpell)
		{
			PendingText = tutorialCastSpellText;
		}
		else if (state == TutorialState.TurnPages)
		{
			bool show = book.PageCount > 1 && book.DisplayingPage;
			PendingText = show ? tutorialTurnPagesText : "";
		}
		else if (state == TutorialState.Done)
		{
			PendingText = "";
		}

		if (Time.time - pendingTextChangedTime >= textLag)
		{
			Text = pendingText;
		}
	}

	private void SetState(TutorialState newState)
	{
		state = newState;
	}
}