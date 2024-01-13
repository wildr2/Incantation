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
}
