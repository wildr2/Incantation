using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowHighlight : MonoBehaviour
{
	private SpriteRenderer spriteRenderer;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		Card card = GameManager.Instance.CurrentCard;
		spriteRenderer.color = card ? card.GetGlowColor() : Color.clear;
	}
}
