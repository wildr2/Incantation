using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Incantor : MonoBehaviour
{
	public Text incantationText;
	private Book book;
	private string inputText;
	private string incantation;
	private float startFadeTime = -1;
	ScoreIncantationResponse scoreResponse;

	public AudioClip lightFireSpellSound;
	public AudioClip extinguishFireSpellSound;

	private void Awake()
	{
		book = FindObjectOfType<Book>();
	}

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
		SpellID spell = GetHighestScoreSpellID(scoreResponse.spellScores);
		Debug.Log(string.Format("rwdbg {0} {1} {2}", incantation, spell, string.Join(", ", response.spellScores)));
	}

	private Spell DetermineSpell(out float intensity)
	{
		SpellID spellID = GetHighestScoreSpellID(scoreResponse.spellScores);
		float score = scoreResponse.spellScores[(int)spellID];

		// TODO: possibly handle score thresholding/normalization on server.
		const float minSuccessScore = 0.9f;
		const float maxIntensityScore = 1.2f;
		if (score < minSuccessScore)
		{
			spellID = SpellID.Generic;
			intensity = Util.Map(0.0f, minSuccessScore, 0.0f, 1.0f, score);
		}
		else
		{
			intensity = Util.Map(minSuccessScore, maxIntensityScore, 0.0f, 1.0f, score);
		}

		return Player.Instance.spells[spellID];
	}

	private void CastSpell()
	{
		Spell spell = DetermineSpell(out float intensity);

		// Cast the spell on a single target that can be affected by it.
		SpellTarget[] targets = FindObjectsOfType<SpellTarget>();
		Util.Shuffle(targets);

		foreach (SpellTarget target in targets)
		{
			if (spell.TryCastSpell(target, intensity))
			{
				break;
			}
		}
	}

	private SpellID GetHighestScoreSpellID(float[] scores)
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
		return (SpellID)bestIndex;
	}
}
