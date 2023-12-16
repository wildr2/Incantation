using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Incantor : MonoBehaviour
{
	public Text incantationText;
	public Book book;
	private string inputText;
	private string incantation;
	private float startFadeTime = -1;
	private float score;

	public AudioClip lightFireSpellSound;
	public AudioClip extinguishFireSpellSound;

	private void Update()
	{
		if (!UpdateFade())
		{
			UpdateInput();
		}
	}

	private bool UpdateFade()
	{
		float fadeDuration = 0.0f;
		bool fading = startFadeTime >= 0;
		float fadeT = !fading ? 1 :
			fadeDuration <= 0 ? 1 : Mathf.Min(1, (Time.time - startFadeTime) / fadeDuration);
		if (fadeT < 1)
		{
			float alpha = 1.0f - Mathf.Pow(fadeT, 2.0f);
			incantationText.color = Util.SetAlpha(incantationText.color, alpha);
		}
		else if (fading)
		{
			incantationText.color = Util.SetAlpha(incantationText.color, 0);
			inputText = "";
			startFadeTime = -1;
			CastSpell(score);
		}
		else
		{
			incantationText.color = Util.SetAlpha(incantationText.color, 1);
		}
		return fading;
	}

	private void UpdateInput()
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
				book.Close();
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape) || book.IsOpen())
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
		startFadeTime = Time.time;
		this.score = score;
		Debug.Log(string.Format("rwdbg {0} {1}", incantation, score));
	}

	private void CastSpell(float score)
	{
		Card card = FindAnyObjectByType<Card>();
		AmbientFire fire = FindAnyObjectByType<AmbientFire>();

		bool lightFire = score > 0.9f;

		if (lightFire)
		{
			float intensity = Util.Map(0.9f, 1.2f, 0.0f, 1.0f, score);
			if (!card.IsFireLit() || (!fire || fire.IsLit()))
			{
				card.LightFire(intensity);
				card.Shake(Util.Map(0.9f, 1.2f, 0.5f, 1.0f, score));
				SFXManager.Play(lightFireSpellSound, MixerGroup.Magic);
			}
			else if (fire && !fire.IsLit())
			{
				fire.Light();
				SFXManager.Play(lightFireSpellSound, MixerGroup.Magic, fire.transform.position);
			}
		}
		else
		{
			if (card.IsFireLit())
			{
				card.ExtinguishFire();
				SFXManager.Play(extinguishFireSpellSound, MixerGroup.Magic);
				card.Shake(Util.Map(0.9f, 1.2f, 0.5f, 1.0f, score));
			}
			else if (fire && fire.IsLit())
			{
				fire.Extinguish();
				SFXManager.Play(extinguishFireSpellSound, MixerGroup.Magic, fire.transform.position);
			}
			else 
			{
				card.Shake(Util.Map(0.0f, 0.9f, 0.0f, 0.3f, score));
			}
		}
	}
}
