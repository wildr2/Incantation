using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Deck : MonoBehaviour
{
	public Card[] cardPrefabs;
	public float[] weights;
	public int initialCount = 3;
	private List<Card> deckCardPrefabs;
	private int drawIndex;

	public int DrawPileCount => deckCardPrefabs.Count - drawIndex;
	public int DiscardPileCount => deckCardPrefabs.Count - DrawPileCount;
	public int Count => deckCardPrefabs.Count;
	public int DrawIndex => drawIndex;

	private void Awake()
	{
		deckCardPrefabs = new List<Card>(initialCount);
		for (int i = 0; i < initialCount; ++i)
		{
			AddRandomCard();
		}
	}

	public void AddRandomCard()
	{
		int index = Util.WeightedRandom(weights);
		deckCardPrefabs.Add(cardPrefabs[index]);
	}

	public void ReShuffle()
	{
		Util.Shuffle(deckCardPrefabs);
		drawIndex = 0;
	}

	public Card Draw()
	{
		return DrawPileCount > 0 ? deckCardPrefabs[drawIndex++] : null;
	}
}
