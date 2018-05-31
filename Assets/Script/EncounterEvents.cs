using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class EncounterEvents : MonoBehaviour
{
	public void WillpowerTest(int value)
	{
		
	}

	public void IntellectTest(int value)
	{
		
	}

	public void CombatTest(int value)
	{
	
	}

	public void AgilityTest(int value)
	{
		ChaosBag.ChaosTokenType chaosToken;
		bool bSucceed = GameLogic.Get().SkillTest(SkillType.Agility, value, out chaosToken);

		if (bSucceed)
		{
			// Succeed!
			
		}
		else
		{
			// Failed..
		
		}

		GameLogic.Get().AfterSkillTest(bSucceed, chaosToken);
	}
}
