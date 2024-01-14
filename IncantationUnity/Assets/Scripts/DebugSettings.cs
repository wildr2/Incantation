using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSettings : Singleton<DebugSettings>
{
	public bool enableDebugIncantations;
	public bool debugIncantationsBypassCirumstances;
	public bool enableIncantationRules;
	public bool enableMagicServer;
	public bool enableCardDebugText;
	public bool enableSpellDebugPrints;
	public bool enableCardDealing;
	public bool debugIncantationDefs;
	public bool debugDeckBuilding;
	public bool disableMusic;
	public bool runTests;
	public bool enableCustomSeed;
	public int customSeed;
	public int Seed { get; private set; }
	public int OverrideSeed { get; set; }

	private void Awake()
	{
		if (Instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			DontDestroyOnLoad(this);
			SceneManager.sceneLoaded += OnSceneLoaded;
			InitSeed();
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		InitSeed();
	}

	private void InitSeed()
	{
		if (OverrideSeed >= 0)
		{
			Seed = OverrideSeed;
		}
		else
		{
			Seed = enableCustomSeed ? customSeed : (int)(System.DateTime.Now.Ticks & 0x7FFFFFFF);
		}
		Random.InitState(Seed);
	}
}
