using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteLighting : MonoBehaviour
{
	private SpriteRenderer spriteRenderer;
	private Color initialColor;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		initialColor = spriteRenderer.color;
	}

	private void Update()
	{
		RoomLighting rl = RoomLighting.Instance;
		spriteRenderer.color = Color.Lerp(Color.black, rl.TintColor(initialColor), rl.Brightness);
	}
}
