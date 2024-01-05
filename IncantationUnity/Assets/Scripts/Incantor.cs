using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Incantor : MonoBehaviour
{
	public Text incantationText;
	private BookProp book;
	private string inputText;
	private string incantation;
	private float startFadeTime = -1;
	ScoreIncantationResponse scoreResponse;

	private void Awake()
	{
		book = FindObjectOfType<BookProp>();
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
			TryCastSpell();
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

				if (DebugSettings.Instance.enableMagicServer)
				{
					MagicClient magicClient = FindObjectOfType<MagicClient>();
					magicClient.ScoreIncantation(incantation, ScoreIncantationCallback);
				}
				else if (DebugSettings.Instance.enableIncantationRules)
				{
					ScoreIncantationCallback(CreateIncantationRulesResponse());
				}
				else
				{
					ScoreIncantationCallback(CreateDummyResponse());
				}
			}
		}

		incantationText.text = inputText;
	}

	private ScoreIncantationResponse CreateIncantationRulesResponse()
	{
		SpellTarget[] targets = FindObjectsOfType<SpellTarget>();

		ScoreIncantationResponse response = new ScoreIncantationResponse();
		int count = Util.GetEnumCount<SpellID>();
		response.spellScores = new float[count];

		for (int i = 0; i < count; ++i)
		{
			if (Player.Instance.spells.TryGetValue((SpellID)i, out Spell spell))
			{
				response.spellScores[i] =
					(spell.CheckIncantation(incantation) ? Random.Range(0.9f, 1.2f) : 0.0f) *
					(spell.IsTargettable(targets) ? 1 : 0);
			}
			else
			{
				response.spellScores[i] = 0.0f;
			}
		}

		return response;
	}

	private ScoreIncantationResponse CreateDummyResponse()
	{
		ScoreIncantationResponse response = new ScoreIncantationResponse();
		int count = Util.GetEnumCount<SpellID>();
		response.spellScores = new float[count];
		for (int i = 0; i < count; ++i)
		{
			response.spellScores[i] = Random.Range(0.0f, 0.9f);
		}
		return response;
	}

	private void DebugScoreIncantation(ScoreIncantationResponse response)
	{
		Player player = Player.Instance;
		foreach (Spell spell in player.spells.Values)
		{
			if (spell.debugIncantation == incantation)
			{
				response.spellScores[(int)spell.SpellID] = Random.Range(0.9f, 1.2f);
			}
		}
	}

	private void ScoreIncantationCallback(ScoreIncantationResponse response)
	{
		if (DebugSettings.Instance.enableDebugIncantations)
		{
			DebugScoreIncantation(response);
		}

		startFadeTime = Time.time;
		scoreResponse = response;
	}

	private Spell DetermineSpell(out float intensity)
	{
		SpellID spellID = GetHighestScoreSpellID(scoreResponse.spellScores);
		float score = scoreResponse.spellScores[(int)spellID];

		// TODO: possibly handle score thresholding/normalization on server.
		const float minSuccessScore = 0.9f;
		const float maxIntensityScore = 1.2f;
		if (score <= 0.0f)
		{
		}
		if (score < minSuccessScore)
		{
			spellID = SpellID.Generic;
			intensity = Util.Map(0.0f, minSuccessScore, 0.0f, 1.0f, score);
		}
		else
		{
			intensity = Util.Map(minSuccessScore, maxIntensityScore, 0.0f, 1.0f, score);
		}

		return intensity > 0 ? Player.Instance.spells[spellID] : null;
	}

	private void TryCastSpell()
	{
		Spell spell = DetermineSpell(out float intensity);
		if (spell == null)
		{
			return;
		}

		// Cast the spell on a single target that can be affected by it.
		SpellTarget[] targets = FindObjectsOfType<SpellTarget>();
		Util.Shuffle(targets);
		System.Array.Sort(targets, new SpellTarget.PriorityComparer());

		SpellTarget castTarget = null;
		foreach (SpellTarget target in targets)
		{
			if (spell.TryCastSpell(target, intensity))
			{
				castTarget = target;
				break;
			}
		}
		if (castTarget)
		{
			if (DebugSettings.Instance.enableSpellDebugPrints)
			{
				Debug.Log(string.Format("Cast '{0}' => {1} ({2}) at {3}", incantation, spell.SpellID, intensity, castTarget));
			}
		}
		else
		{
			// No target for spell, instead cast the generic spell at the same intensity.
			Spell genericSpell = Player.Instance.spells[SpellID.Generic];
			foreach (SpellTarget target in targets)
			{
				if (genericSpell.TryCastSpell(target, intensity))
				{
					castTarget = target;
					break;
				}
			}
			if (DebugSettings.Instance.enableSpellDebugPrints)
			{
				Debug.Log(string.Format("Cast '{0}' => {1} ({2}) no target => {3} ({4}) at {5}", incantation, spell.SpellID, intensity, genericSpell.SpellID, intensity, castTarget));
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
