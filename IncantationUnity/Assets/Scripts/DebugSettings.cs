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
	public bool enableSeed;
	public int seed = -1;

	public int GetSeed()
	{
		if (seed < 0)
		{
			seed = (int)(System.DateTime.Now.Ticks & 0x7FFFFFFF);
		}
		return seed;
	}

	private void Awake()
	{
		if (enableSeed)
		{
			Random.InitState(GetSeed());
		}
	}
}
