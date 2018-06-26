/********************************************************************
	created:	2018/06/26
	created:	26:6:2018   23:00
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_deduction : PlayerCardLogic
{
	private bool m_isActive = false;

	public override void OnSkillTest(int result)
	{
		if (Player.Get().m_currentAction.Peek() != PlayerAction.Investigate || result < 0)
		{
			return;
		}

		Player.Get().m_cluesDuringInvest += 1;
		m_isActive = true;
		GameLogic.Get().OutputGameLog(string.Format("{0}打出<演绎法>，本次调查线索+1\n", Player.Get().m_investigatorCard.m_cardName));
	}

	public override void OnDiscard(Card card)
	{
		if (m_isActive)
		{
			Player.Get().m_cluesDuringInvest -= 1;
			m_isActive = false;
		}
	}
}
