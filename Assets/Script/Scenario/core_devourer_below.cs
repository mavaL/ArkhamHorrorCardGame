using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class core_devourer_below : scenario_base
{
	void Awake()
	{
		GameLogic.Get().m_currentScenario = this;
	}

	void Start()
	{
		
	}

	public override void ShowPlayInfo()
	{
		
	}

	public override int GetChaosTokenEffect(ChaosBag.ChaosTokenType t)
	{
		Assert.IsTrue(false, "Assert failed in GetChaosTokenEffect()!!");
		return -1;
	}

	public override void AfterSkillTestFailed(ChaosBag.ChaosTokenType t)
	{
		
	}

	public override void AdvanceAct()
	{
		throw new System.NotImplementedException();
	}

	public override void AdvanceAgenda()
	{
		throw new System.NotImplementedException();
	}
}
