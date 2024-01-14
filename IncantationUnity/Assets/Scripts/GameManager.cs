using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
	Init,
	PreRound,
	Round,
}

public class GameManager : Singleton<GameManager>
{
	// Delay after card completion before the start of the next card sequence.
	public float nextCardDelay;
	// Delay from the start of the next card sequence before the removal of the card.
	public float removeCardDelay;
	// Replaces removeCardDelaySkipped when card was skipped.
	public float removeCardDelaySkipped;
	// Delay from start of the next card sequence before the playing of dealCardSFX;
	public float dealCardDelay;
	// Replaces dealCardDelay when card was skipped.
	public float dealCardDelaySkipped;
	// Delay from the playing of dealCardSFX before spawning the next card.
	public float spawnCardDelay;
	public float secondsBeforeCanSkip;
	public float preRoundDuration;
	public Transform cardDealPos;
	public float maxCardDealPosOffset;
	public float maxCardDealRotationDeg; 
	public AudioClip dealCardSFX;
	public AudioClip shuffleDeckSFX;
	public AudioClip skipCardSFX;
	public Text helpText;
	public Text scoreText;

	private GameState state = GameState.Init;
	private Deck deck;
	private BookProp book;
	private float enterStateTime;
	private bool dealingNextCard;
	public Card CurrentCard { get; private set; }
	private float currentCardDealTime = -1;
	private float currentCardDoneTime = -1;
	private bool skippedCurrentCard;
	private float currentCardBecameSkippableTime = -1;
	private int cardsCompleted;
	public System.Action onCardCompleted;
	public System.Action onCardSkipped;

	private float StateTime => Time.time - enterStateTime;

	private void Awake()
	{
		deck = GetComponent<Deck>();
		book = FindObjectOfType<BookProp>();
	}

	private void Update()
	{
		UpdateCardDealing();
		UpdateText();

		if (!DebugSettings.Instance.enableCardDealing)
		{
			CurrentCard = FindAnyObjectByType<Card>();
		}

		if (DebugSettings.Instance.debugDeckBuilding)
		{
			DebugDeckBuilding();
		}
	}

	private void DebugDeckBuilding()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			deck.AddRandomCard(Player.Instance.GetLearnedSpells());
			Card cardPrefab = deck.DeckCardPrefabs[deck.DeckCardPrefabs.Count - 1];
			Spell spell = Player.Instance.spells[cardPrefab.goalSpellID];

			string str = string.Format("{0}\n---------\n", spell.GetDescription());
			Debug.Log(str);

			foreach (Card card in deck.DeckCardPrefabs)
			{
				Player.Instance.spells[card.goalSpellID].seen = true;
			}
		}
	}

	private void UpdateCardDealing()
	{
		if (!DebugSettings.Instance.enableCardDealing)
		{
			return;
		}

		if (state == GameState.Init)
		{
			NextRound();
		}
		else if (state == GameState.PreRound)
		{
			UpdatePreRound();
		}
		else
		{
			UpdateRound();
		}
	}

	private void UpdatePreRound()
	{
		if (StateTime > preRoundDuration)
		{
			NextCard();
			SetState(GameState.Round);
		}
	}

	private void UpdateRound()
	{
		if (CurrentCard && !dealingNextCard)
		{
			if (currentCardDoneTime < 0)
			{
				if (CurrentCard.IsComplete())
				{
					currentCardDoneTime = Time.time;
				}
				else if (CurrentCard.IsSkippable() || Time.time - currentCardDealTime > secondsBeforeCanSkip)
				{
					if (currentCardBecameSkippableTime < 0)
					{
						currentCardBecameSkippableTime = Time.time;
					}
					if (Input.GetKeyDown(KeyCode.Space) && CanSkip())
					{
						SkipCard();
					}
				}
			}
			else if (Time.time - currentCardDoneTime > nextCardDelay)
			{
				if (deck.DrawPileCount > 0)
				{
					NextCard();
				}
				else
				{
					NextRound();
				}
			}
		}
	}

	private void UpdateText()
	{
		const string skipCardText = "press space to skip card";

		helpText.text = "";

		TutorialText tutorialText = GetComponent<TutorialText>();

		bool showSkipText =
			state == GameState.Round &&
			currentCardDoneTime < 0 &&
			currentCardBecameSkippableTime >= 0 &&
			CanSkip();

		if (showSkipText)
		{
			helpText.text = skipCardText;
		}
		else if (tutorialText.Text != "")
		{
			helpText.text = tutorialText.Text;
		}

		scoreText.text = string.Format("{0}\t\t{1}", Util.FormatTimeAsMinSec(Time.timeSinceLevelLoad), cardsCompleted);
	}

	public bool CanSkip()
	{
		return Incantor.Instance.InputText.Length == 0;
	}

	private void SkipCard()
	{
		currentCardDoneTime = Time.time - nextCardDelay;
		skippedCurrentCard = true;
		if (onCardSkipped != null)
		{
			onCardSkipped();
		}
		SFXManager.Play(skipCardSFX);
	}

	private void NextRound()
	{
		Debug.Log("Next round");
		if (CurrentCard)
		{
			Destroy(CurrentCard.gameObject);
			if (!skippedCurrentCard)
			{
				OnCardCompletion();
			}

			int newCardCount = deck.Count >= 5 ? 2 : 1;
			for (int i = 0; i < newCardCount; ++i)
			{
				deck.AddRandomCard(Player.Instance.GetLearnedSpells());
			}
			deck.ReShuffle();
		}

		SFXManager.Play(shuffleDeckSFX);
		SetState(GameState.PreRound);
	}

	private void NextCard()
	{
		Debug.Log(string.Format("Next card ({0}/{1})", deck.DiscardPileCount + 1, deck.Count));

		dealingNextCard = true;

		// Spawn card in advance of displaying it to hide visual initialization issue.
		Card cardPrefab = deck.Draw();
		Card newCard = Instantiate(cardPrefab, transform);
		newCard.transform.localScale = Vector3.zero;

		float removeDelay = skippedCurrentCard ? removeCardDelaySkipped : removeCardDelay;
		float dealDelay = skippedCurrentCard ? dealCardDelaySkipped : dealCardDelay;

		StartCoroutine(CoroutineUtil.DoAfterDelay(() =>
		{
			if (CurrentCard)
			{
				if (!skippedCurrentCard)
				{
					OnCardCompletion();
				}
				Destroy(CurrentCard.gameObject);
			}
		}, removeDelay));

		StartCoroutine(CoroutineUtil.DoAfterDelay(() =>
		{
			SFXManager.Play(dealCardSFX);

			StartCoroutine(CoroutineUtil.DoAfterDelay(() =>
			{
				CurrentCard = newCard;
				CurrentCard.transform.localScale = Vector3.one;
				CurrentCard.transform.position = cardDealPos.position + (Vector3)Util.RandomDir2D() * maxCardDealPosOffset;
				CurrentCard.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-maxCardDealRotationDeg, maxCardDealRotationDeg));
				currentCardDoneTime = -1;
				currentCardBecameSkippableTime = -1;
				skippedCurrentCard = false;
				dealingNextCard = false;
				currentCardDealTime = Time.time;
			}, spawnCardDelay));
		}, dealDelay));
	}

	private void OnCardCompletion()
	{
		++cardsCompleted;
		if (onCardCompleted != null)
		{
			onCardCompleted();
		}

		if (cardsCompleted == 5 && !book.HasPage(SpellID.SummonStendarii))
		{
			book.AddPage(SpellID.SummonStendarii);
		}
	}

	private void SetState(GameState state)
	{
		this.state = state;
		enterStateTime = Time.time;
	}
}
