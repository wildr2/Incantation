using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
	public Card[] allCardPrefabs;
	public int initialCount = 3;
	public List<Card> cardPrefabs;
	private int drawIndex;

	public int DrawPileCount => cardPrefabs.Count - drawIndex;
	public int DiscardPileCount => cardPrefabs.Count - DrawPileCount;
	public int Count => cardPrefabs.Count;
	public int DrawIndex => drawIndex;

	private void Awake()
	{
		cardPrefabs = new List<Card>(initialCount);
		for (int i = 0; i < initialCount; ++i)
		{
			AddRandomCard();
		}
	}

	public void AddRandomCard()
	{
		cardPrefabs.Add(allCardPrefabs[Random.Range(0, allCardPrefabs.Length)]);
	}

	public void ReShuffle()
	{
		Util.Shuffle(cardPrefabs);
		drawIndex = 0;
	}

	public Card Draw()
	{
		return DrawPileCount > 0 ? cardPrefabs[drawIndex++] : null;
	}
}
