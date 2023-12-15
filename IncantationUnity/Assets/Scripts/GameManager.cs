using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public SpriteRenderer deskHighlights;

	private void Update()
	{
		Card card = FindObjectOfType<Card>();
		deskHighlights.color = Util.SetAlpha(deskHighlights.color, card.glow);
	}
}
