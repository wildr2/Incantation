using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SpellType
{
	LightFire,
	ExtinguishFire,
}

public class Incantor : MonoBehaviour
{
	public Text incantationText;
	public Book book;
	private string inputText;
	private string incantation;
	private float startFadeTime = -1;
	ScoreIncantationResponse scoreResponse;

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
			CastSpell();
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
				magicClient.ScoreIncantation(incantation, CheckSpellCallback);
			}
		}

		incantationText.text = inputText;
	}

	private void CheckSpellCallback(ScoreIncantationResponse response)
	{
		startFadeTime = Time.time;
		scoreResponse = response;
		SpellType spell = GetHighestScoreSpellType(scoreResponse.spellScores);
		Debug.Log(string.Format("rwdbg {0} {1} {2}", incantation, spell, string.Join(", ", response.spellScores)));
	}

	private void CastSpell()
	{
		Card card = FindAnyObjectByType<Card>();
		AmbientFire fire = FindAnyObjectByType<AmbientFire>();

		SpellType spell = GetHighestScoreSpellType(scoreResponse.spellScores);
		float score = scoreResponse.spellScores[(int)spell];
		if (score < 0.9f)
		{
			return;
		}

		if (spell == SpellType.LightFire)
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
		else if (spell == SpellType.ExtinguishFire)
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

	private SpellType GetHighestScoreSpellType(float[] scores)
	{
		float bestScore = 0;
		int bestIndex = 0;
		for (int i = 0; i < scores.Length; ++i)
		{
			if (scores[i] > bestScore)
			{
				bestScore = scores[i];
				bestIndex = i;
			}
		}
		return (SpellType)bestIndex;
	}
}
