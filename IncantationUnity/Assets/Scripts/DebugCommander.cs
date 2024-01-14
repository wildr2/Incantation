using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DebugCommand
{
	None,
	CopySeed,
	MusicOff,
	MusicOn,
	RestartGame,
	QuitGame,
}

public class DebugCommander : MonoBehaviour
{
	public AudioClip[] castSFX;
	public float castDelay;

	private string[] incantations = new string[]
	{
		"",
		"copy seed",
		"music off",
		"music on",
		"restart game",
		"quit game",
	};

	public bool CheckIncantation(string incantation, out DebugCommand command)
	{
		if (incantation.Length == 0)
		{
			command = DebugCommand.None;
			return false;
		}
		int i = System.Array.FindIndex(incantations, inc => inc == incantation);
		command = i >= 0 ? (DebugCommand)i : DebugCommand.None;
		return i >= 0;
	}

	public void CastDebugCommand(DebugCommand command)
	{
		SFXManager.Play(castSFX, MixerGroup.Magic);
		StartCoroutine(CoroutineUtil.DoAfterDelay(() => DoCommand(command), castDelay));
	}

	private void DoCommand(DebugCommand command)
	{
		switch (command)
		{
			case DebugCommand.CopySeed:
				GUIUtility.systemCopyBuffer = DebugSettings.Instance.Seed.ToString();
				break;
			case DebugCommand.MusicOff:
				Music.Instance.SetEnabled(false);
				break;
			case DebugCommand.MusicOn:
				Music.Instance.SetEnabled(true);
				break;
			case DebugCommand.RestartGame:
				SceneManager.LoadScene(0);
				break;
			case DebugCommand.QuitGame:
				Application.Quit();
				break;
		}
	}
}
