/********************************************************************
	created:	2018/07/05
	created:	5:7:2018   0:29
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_otoole : InvestigatorLogic
{
	public override int OnApplyElderSignEffect()
	{
		return 2;
	}

	public override void OnElderSignSkillTestResult(bool bSucceed)
	{
		if (bSucceed)
		{
			Player.Get().m_resources += 2;
			GameLogic.Get().OutputGameLog("奥图尔触发能力获得了2资源！\n");
		}
	}
}
