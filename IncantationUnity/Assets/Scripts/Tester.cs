using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
	private void Start()
	{
		if (DebugSettings.Instance.runTests)
		{
			TestSpells();
		}
	}

	private void TestSpells()
	{
		IncantationDef a = IncantationDef.TestDef(2);
		a.rules[0].ruleType = IncantationRuleType.ContainsLetter;
		a.rules[0].letter = 'a';
		a.rules[1].ruleType = IncantationRuleType.ContainsLetter;
		a.rules[1].letter = 'b';
		a.requiredCircumstances = IncantationCircumstance.Raining;

		IncantationDef b = IncantationDef.TestDef(1);
		b.rules[0].ruleType = IncantationRuleType.ContainsLetter;
		b.rules[0].letter = 'b';

		IncantationDef c = IncantationDef.TestDef(1);
		c.rules[0].ruleType = IncantationRuleType.ContainsLetter;
		c.rules[0].letter = 'c';

		Debug.Assert(a.IsEquivalent(a));
		Debug.Assert(!a.Includes(c));
		Debug.Assert(a.Includes(b));
		Debug.Assert(!b.Includes(a));
		Debug.Assert(a.Passes("batch", IncantationCircumstance.Raining));
		Debug.Assert(!a.Passes("batch", IncantationCircumstance.Dark));
		Debug.Assert(b.Passes("batch", IncantationCircumstance.Raining));
		Debug.Assert(b.Passes("batch", 0));
		Debug.Assert(!a.Passes("catch", IncantationCircumstance.Raining));

		IncantationDef d = IncantationDef.TestDef(1);
		d.rules[0].ruleType = IncantationRuleType.ContainsLetter;
		d.rules[0].letter = 'd';
		d.requiredCircumstances = IncantationCircumstance.Dark;

		IncantationDef e = IncantationDef.TestDef(1);
		e.rules[0].ruleType = IncantationRuleType.StartsWithLetter;
		e.rules[0].letter = 'd';
		e.requiredCircumstances = IncantationCircumstance.PitchBlack;

		Debug.Assert(e.Includes(d));
		Debug.Assert(!d.Includes(e));
	}
}
