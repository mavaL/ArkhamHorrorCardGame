/********************************************************************
	created:	2018/07/04
	created:	4:7:2018   0:50
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_working_a_hunch : PlayerCardLogic
{
	public override void OnReveal(Card card)
	{
		Player.Get().m_clues += 1;
		Player.Get().m_currentLocation.m_clues -= 1;
		var pc = GetComponent<PlayerCard>();

		GameLogic.Get().OutputGameLog(string.Format("{0}打出<预感指引>，花费{1}资源，获取了1线索\n", Player.Get().m_investigatorCard.m_cardName, pc.m_cost));

		Player.Get().m_currentAction.Pop();
	}

	public override bool CanTrigger()
	{
		return Player.Get().m_currentLocation.m_clues > 0;
	}
}
