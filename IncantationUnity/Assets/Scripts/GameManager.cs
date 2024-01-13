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
	public float nextCardDelay;
	public float removeCardDelay;
	public float spawnCardDelay;
	public float secondsBeforeCanSkip;
	public float preRoundDuration;
	public Transform cardDealPos;
	public float maxCardDealPosOffset;
	public float maxCardDealRotationDeg; 
	public AudioClip dealCardSFX;
	public AudioClip shuffleDeckSFX;
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
	private float currentCardBecameSkippableTime = -1;
	private int cardsCompleted;
	public System.Action onCardCompleted;

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
					else if (Input.GetKeyDown(KeyCode.Space))
					{
						currentCardDoneTime = Time.time - nextCardDelay;
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
			currentCardBecameSkippableTime >= 0;

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

	private void NextRound()
	{
		Debug.Log("Next round");
		if (CurrentCard)
		{
			Destroy(CurrentCard.gameObject);
			if (currentCardBecameSkippableTime < 0)
			{
				OnCardCompletion();
			}

			int newCardCount = deck.Count >= 5 ? 2 : 1;
			for (int i = 0; i < newCardCount; ++i)
			{
				deck.AddRandomCard();
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
		SFXManager.Play(dealCardSFX);

		// Spawn card in advance of displaying it to hide visual initialization issue.
		Card cardPrefab = deck.Draw();
		Card newCard = Instantiate(cardPrefab, transform);
		newCard.transform.localScale = Vector3.zero;

		StartCoroutine(CoroutineUtil.DoAfterDelay(() =>
		{
			if (CurrentCard)
			{
				if (currentCardBecameSkippableTime < 0)
				{
					OnCardCompletion();
				}
				Destroy(CurrentCard.gameObject);
			}
		}, removeCardDelay));
		StartCoroutine(CoroutineUtil.DoAfterDelay(() =>
		{
			CurrentCard = newCard;
			CurrentCard.transform.localScale = Vector3.one;
			CurrentCard.transform.position = cardDealPos.position + (Vector3)Util.RandomDir2D() * maxCardDealPosOffset;
			CurrentCard.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-maxCardDealRotationDeg, maxCardDealRotationDeg));
			currentCardDoneTime = -1;
			currentCardBecameSkippableTime = -1;
			dealingNextCard = false;
			currentCardDealTime = Time.time;
		}, spawnCardDelay));
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
