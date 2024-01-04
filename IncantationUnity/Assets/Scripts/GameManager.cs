using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public float cardRemovalDelay = 3.0f;
	public SpriteRenderer deskHighlights;
	public Deck deck;
	public Transform cardDealPos;
	public float maxCardDealPosOffset;
	public float maxCardDealRotationDeg; 
	private Card currentCard;
	private float currentCardDoneTime = -1;

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
		if (!currentCard)
		{
			NextCard();
		}
		else
		{
			if (currentCardDoneTime < 0)
			{
				if (currentCard.IsComplete() || currentCard.IsSkippable())
				{
					currentCardDoneTime = Time.time;
				}
			}
			else if (Time.time - currentCardDoneTime > cardRemovalDelay)
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

	private void NextCard()
	{
		Debug.Log(string.Format("Next card ({0}/{1})", deck.DiscardPileCount + 1, deck.Count));

		if (currentCard)
		{
			Destroy(currentCard.gameObject);
		}

		Card cardPrefab = deck.Draw();
		currentCard = Instantiate(cardPrefab, transform);
		currentCard.transform.position = cardDealPos.position + (Vector3)Util.RandomDir2D() * maxCardDealPosOffset;
		transform.rotation = Quaternion.Euler(0, 0, Random.Range(-maxCardDealRotationDeg, maxCardDealRotationDeg));
		currentCardDoneTime = -1;
	}

	private void NextRound()
	{
		Debug.Log("Next round");
		deck.AddRandomCard();
		deck.AddRandomCard();
		deck.ReShuffle();
		NextCard();
	}
}
