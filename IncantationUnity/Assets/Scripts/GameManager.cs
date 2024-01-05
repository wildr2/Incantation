using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public float preRoundDuration;
	public SpriteRenderer deskHighlights;
	public Deck deck;
	public Transform cardDealPos;
	public float maxCardDealPosOffset;
	public float maxCardDealRotationDeg; 
	public AudioClip dealCardSFX;
	public AudioClip shuffleDeckSFX;
	private GameState state = GameState.Init;
	private float enterStateTime;
	private bool dealingNextCard;
	public Card CurrentCard { get; private set; }
	private float currentCardDoneTime = -1;

	private float StateTime => Time.time - enterStateTime;

	private void Awake()
	{
		deck = GetComponent<Deck>();
	}

	private void Update()
	{
		UpdateDeskHighlights();
		UpdateCardDealing();
	}

	private void UpdateDeskHighlights()
	{
		Card card = FindObjectOfType<Card>();
		if (card)
		{
			deskHighlights.color = Util.SetAlpha(deskHighlights.color, card.GetGlowIntensity());
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
				if (CurrentCard.IsComplete() || CurrentCard.IsSkippable())
				{
					currentCardDoneTime = Time.time;
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

	private void NextRound()
	{
		Debug.Log("Next round");
		if (CurrentCard)
		{
			Destroy(CurrentCard.gameObject);

			deck.AddRandomCard();
			deck.AddRandomCard();
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

		StartCoroutine(CoroutineUtil.DoAfterDelay(() =>
		{
			if (CurrentCard)
			{
				Destroy(CurrentCard.gameObject);
			}
		}, removeCardDelay));
		StartCoroutine(CoroutineUtil.DoAfterDelay(() =>
		{
			Card cardPrefab = deck.Draw();
			CurrentCard = Instantiate(cardPrefab, transform);
			CurrentCard.transform.position = cardDealPos.position + (Vector3)Util.RandomDir2D() * maxCardDealPosOffset;
			transform.rotation = Quaternion.Euler(0, 0, Random.Range(-maxCardDealRotationDeg, maxCardDealRotationDeg));
			currentCardDoneTime = -1;
			dealingNextCard = false;
		}, spawnCardDelay));
	}

	private void SetState(GameState state)
	{
		this.state = state;
		enterStateTime = Time.time;
	}
}
