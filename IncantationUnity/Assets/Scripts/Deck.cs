using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Deck : MonoBehaviour
{
	public Card[] cardPrefabs;
	public float[] weights;
	public int initialCount = 3;
	public List<Card> DeckCardPrefabs { get; private set; }
	private int drawIndex;

	public int DrawPileCount => DeckCardPrefabs.Count - drawIndex;
	public int DiscardPileCount => DeckCardPrefabs.Count - DrawPileCount;
	public int Count => DeckCardPrefabs.Count;
	public int DrawIndex => drawIndex;

	private void Awake()
	{
		DeckCardPrefabs = new List<Card>(initialCount);
	}

	private void Start()
	{
		for (int i = 0; i < initialCount; ++i)
		{
			AddRandomCard(new Spell[] { });
		}
	}

	public void AddRandomCard(IEnumerable<Spell> learnedSpells)
	{
		List<Card> poolCardPrefabs = new List<Card>(cardPrefabs);
		List<float> poolWeights = new List<float>(weights);

		for (int i = poolCardPrefabs.Count - 1; i >= 0; --i)
		{
			SpellID spellID = poolCardPrefabs[i].goalSpellID;
			Spell spell = Player.Instance.spells[spellID];
			if (!spell.IncantationDef.ArePrerequisitesLearned(learnedSpells))
			{
				poolCardPrefabs.RemoveAt(i);
				poolWeights.RemoveAt(i);
			}
		}

		if (poolCardPrefabs.Count == 0)
		{
			Debug.LogError("No cards have prerequisites met!");
			return;
		}

		int index = Util.WeightedRandom(poolWeights);
		DeckCardPrefabs.Add(poolCardPrefabs[index]);
	}

	public void ReShuffle()
	{
		Util.Shuffle(DeckCardPrefabs);
		drawIndex = 0;
	}

	public Card Draw()
	{
		return DrawPileCount > 0 ? DeckCardPrefabs[drawIndex++] : null;
	}
}
