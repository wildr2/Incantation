using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Incantor : MonoBehaviour
{
	public Text incantationText;
	private string inputText;
	private string incantation;

	private void Update()
	{
		foreach (char c in Input.inputString)
		{
			if (c == "\b"[0])
			{
				// Backspace.
				inputText = inputText.Substring(0, Mathf.Max(0, inputText.Length - 1));
			}
			else if (char.IsLetter(c) || c == ' ')
			{
				inputText += c;
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			inputText = "";
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			if (inputText.Length > 0)
			{
				incantation = inputText;
				MagicClient magicClient = FindObjectOfType<MagicClient>();
				magicClient.CheckSpell(incantation, CheckSpellCallback);
			}
		}

		incantationText.text = inputText;
	}

	private void CheckSpellCallback(float score)
	{
		inputText = "";

		Card card = FindAnyObjectByType<Card>();
		if (score > 0.9f)
		{
			float intensity = Util.Map(0.9f, 1.2f, 0.0f, 1.0f, score);
			card.LightFire(intensity);
		}
		else
		{
			card.ExtinguishFire();
		}

		Debug.Log(string.Format("rwdbg {0} {1}", incantation, score));
	}
}
