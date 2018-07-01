/********************************************************************
	created:	2018/06/27
	created:	27:6:2018   0:09
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class core_magnifying_glass : PlayerCardLogic
{
	private UnityAction<int, Card> m_afterSkillTest;

	public override void OnReveal(Card card)
	{
		m_afterSkillTest = new UnityAction<int, Card>(AfterSkillTest);
	}

	public override void OnUseReactiveAsset()
	{
		if(Player.Get().m_currentAction.Contains(PlayerAction.Investigate))
		{
			Player.Get().m_investigatorCard.m_intellect += 1;
			GameLogic.Get().m_afterSkillTest.AddListener(m_afterSkillTest);

			GameLogic.Get().m_currentTiming = EventTiming.None;
			Player.Get().m_currentAction.Pop();
		}
	}

	private void AfterSkillTest(int result, Card target)
	{
		GameLogic.Get().m_afterSkillTest.RemoveListener(m_afterSkillTest);
		Player.Get().m_investigatorCard.m_intellect -= 1;
	}

	public override bool CanTrigger()
	{
		return Player.Get().GetCurrentAction() == PlayerAction.Investigate;
	}
}
