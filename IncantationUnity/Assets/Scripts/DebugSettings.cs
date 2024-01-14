using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public bool runTests;
	public bool enableCustomSeed;
	public int customSeed;
	public int Seed { get; private set; }

	private void Awake()
	{
		Seed = enableCustomSeed ? customSeed : (int)(System.DateTime.Now.Ticks & 0x7FFFFFFF);
		Random.InitState(Seed);
	}
}
