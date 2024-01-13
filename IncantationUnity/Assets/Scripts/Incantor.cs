using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Incantor : MonoBehaviour
{
	// Words must be uppercase, delimited by newlines, and sorted in ascending order.
	public Text incantationText;
	public TextAsset validWordsTextAsset;
	public System.Action onCastSpell;

	private BookProp book;
	private string inputText = "";
	private string incantation;
	private float startFadeTime = -1;
	private ScoreIncantationResponse scoreResponse;
	// Words are uppercase.
	private Dictionary<string, int> wordUseCount = new Dictionary<string, int>();
	// Words are uppercase.
	private string[] validWords;

	public bool IsValidWord(string word)
	{
		string wordUpper = word.ToUpper();
		return System.Array.BinarySearch(validWords, wordUpper) >= 0;
	}

	public bool AreWordsValid(string incantation)
	{
		string[] words = incantation.Split(' ');
		foreach (string word in words)
		{
			if (!IsValidWord(word))
			{
				return false;
			}
		}
		return true;
	}

	private void Awake()
	{
		book = FindObjectOfType<BookProp>();

		string text = validWordsTextAsset.ToString();
		validWords = text.Split(System.Environment.NewLine);
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
			else if (char.IsLetter(c) || (c == ' ' && inputText.Length > 0))
			{
				inputText += c;
				book.Close();
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape) || book.open)
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
		SpellID goalSpellID = GameManager.Instance.CurrentCard ? GameManager.Instance.CurrentCard.goalSpellID : SpellID.Generic;

		ScoreIncantationResponse response = new ScoreIncantationResponse();
		Spell[] spells = Player.Instance.GetSpellsArray();
		response.spellScores = new float[spells.Length];

		bool incantationCastable = IsIncantationCastable(incantation);
		IncantationCircumstance circumstances = CircumstancesChecker.Instance.GetCircumstances();

		for (int i = 0; i < spells.Length; ++i)
		{
			Spell spell = spells[i];
			if (spell == null)
			{
				response.spellScores[i] = 0.0f;
				continue;
			}
			if (spell.SpellID == SpellID.Generic)
			{
				response.spellScores[i] = 0.1f;
			}
			else
			{
				// TODO: sort with comparer to avoid edge cases (e.g. >= 10 ConditionCount).
				bool castable = incantationCastable && spell.CheckIncantation(incantation, circumstances) && spell.IsTargettable(targets);
				response.spellScores[i] = !castable ? 0 : 1 +
					(spell.seen ? 1000 : 0) +
					(spell.priority * 100) +
					(spell.SpellID != goalSpellID ? 1 : 0); // Overlapping incantations should consistently result in the wrong spell!
			}
		}

		// A spell whose incantation includes that of the highest scoring spell should be favored regardless of whether the goal spell.
		// Example:
		//		Spell A: contains 'a', contains 'b', is the goal spell
		//		Spell B: contains 'a'
		//		Spell C: contains 'c'
		//
		//		incantation: "bat"		=> spell A should be picked
		//		incantation: "batch"		=> spell C should be picked
		SpellID bestSpellID = GetHighestScoreSpellID(response.spellScores);
		if (response.spellScores[(int)bestSpellID] > 0)
		{
			Spell bestSpell = spells[(int)bestSpellID];
			for (int i = 0; i < spells.Length; ++i)
			{
				Spell spell = spells[i];
				if (spell != null &&
					spell.SpellID != bestSpellID &&
					response.spellScores[i] > 0 &&
					spell.IncantationDef.Includes(bestSpell.IncantationDef))
				{
					response.spellScores[i] += 2;
				}
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
			response.spellScores[i] = 0.0f;
		}
		return response;
	}

	private void DebugScoreIncantation(ScoreIncantationResponse response)
	{
		Player player = Player.Instance;
		IncantationCircumstance circumstances = CircumstancesChecker.Instance.GetCircumstances();

		foreach (Spell spell in player.spells.Values)
		{
			if (spell.CheckDebugIncantation(incantation, circumstances))
			{
				response.spellScores[(int)spell.SpellID] = float.MaxValue;
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

		//// TODO: possibly handle score thresholding/normalization on server.
		//const float minSuccessScore = 0.9f;
		//const float maxIntensityScore = 1.2f;
		//if (score <= 0.0f)
		//{
		//}
		//if (score < minSuccessScore)
		//{
		//	spellID = SpellID.Generic;
		//	intensity = Util.Map(0.0f, minSuccessScore, 0.0f, 1.0f, score);
		//}
		//else
		//{
		//	intensity = Util.Map(minSuccessScore, maxIntensityScore, 0.0f, 1.0f, score);
		//}

		//return intensity > 0 ? Player.Instance.spells[spellID] : null;

		intensity = 1.0f;
		return score > 0 ? Player.Instance.spells[spellID] : null;
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
		System.Array.Sort(targets, new SpellTarget.SpellDependentPriorityComparer(spell.SpellID));

		SpellCast spellCast = null;
		foreach (SpellTarget target in targets)
		{
			spellCast = spell.TryCastSpell(target, intensity);
			if (spellCast != null)
			{
				break;
			}
		}
		if (spellCast != null)
		{
			if (DebugSettings.Instance.enableSpellDebugPrints)
			{
				Debug.Log(string.Format("Cast '{0}' => {1} ({2}) at {3}", incantation, spell.SpellID, intensity, spellCast.target));
			}

			if (spell.SpellID != SpellID.Generic)
			{
				IncrementWordUseCounts(incantation);
			}
		}
		else
		{
			// No target for spell, instead cast the generic spell at the same intensity.
			Spell genericSpell = Player.Instance.spells[SpellID.Generic];
			foreach (SpellTarget target in targets)
			{
				spellCast = genericSpell.TryCastSpell(target, intensity);
				if (spellCast != null)
				{
					break;
				}
			}
			if (DebugSettings.Instance.enableSpellDebugPrints)
			{
				Debug.Log(string.Format("Cast '{0}' => {1} ({2}) no target => {3} ({4}) at {5}", incantation, spell.SpellID, intensity, genericSpell.SpellID, intensity, spellCast.target));
			}
		}

		if (spellCast != null)
		{
			StartCoroutine(UpdateSpellCast(spellCast));
			if (onCastSpell != null)
			{
				onCastSpell();
			}
		}
	}

	private bool ContainsDuplicateWords(string incantation)
	{
		string[] words = incantation.Split(' ');
		for (int i = 0; i < words.Length; ++i)
		{
			for (int j = i + 1; j < words.Length; ++j)
			{
				if (words[i].ToUpper() == words[j].ToUpper())
				{
					return true;
				}
			}
		}
		return false;
	}

	private int GetMaxWordUseCount(string incantation)
	{
		int max = 0;
		string[] words = incantation.Split(' ');
		foreach (string word in words)
		{
			string wordUpper = word.ToUpper();
			wordUseCount.TryGetValue(wordUpper, out int count);
			max = Mathf.Max(max, count);
		}
		return max;
	}

	private void IncrementWordUseCounts(string incantation)
	{
		string[] words = incantation.Split(' ');
		foreach (string word in words)
		{
			string wordUpper = word.ToUpper();
			wordUseCount.TryGetValue(wordUpper, out int count);
			wordUseCount[wordUpper] = count + 1;
		}
	}

	// Are words valid and unused.
	private bool IsIncantationCastable(string incantation)
	{
		return AreWordsValid(incantation) && GetMaxWordUseCount(incantation) == 0 && !ContainsDuplicateWords(incantation);;
	}

	private IEnumerator UpdateSpellCast(SpellCast spellCast)
	{
		while (spellCast.EffectTime < 0)
		{
			yield return null;
		}

		if (spellCast.effect != null && spellCast.effect.AreConditionsMet())
		{
			spellCast.effect.Apply(spellCast);
		}
		else
		{
			// Lost target during cast delay.
			spellCast.audioSource.Stop();
			Debug.Log("Lost spell cast target during cast delay.");
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
