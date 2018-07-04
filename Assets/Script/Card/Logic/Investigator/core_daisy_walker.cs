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

public class core_daisy_walker : InvestigatorLogic
{
	public override int OnApplyElderSignEffect()
	{
		return 0;
	}

	public override void OnElderSignSkillTestResult(bool bSucceed)
	{
		if (bSucceed)
		{
			int num = Card.HowManyPlayerCardContainTheKeyword(Player.Get().GetHandCards(), Card.Keyword.Tome);

			for (int i = 0; i < num; ++i)
			{
				Player.Get().AddHandCard(GameLogic.Get().DrawPlayerCard());
			}

			GameLogic.Get().OutputGameLog(string.Format("黛西触发能力获得了{0}手牌！\n", num));
		}
	}
}
