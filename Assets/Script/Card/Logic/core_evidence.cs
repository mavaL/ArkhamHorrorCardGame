/********************************************************************
	created:	2018/06/12
	created:	12:6:2018   15:07
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_evidence : PlayerCardLogic
{
	public override void OnReveal(Card card)
	{
		Player.Get().m_clues += 1;
		Player.Get().m_currentLocation.m_clues -= 1;
		var pc = GetComponent<PlayerCard>();

		GameLogic.Get().OutputGameLog(string.Format("{0}打出<证据>，花费{1}资源，获取了1线索\n", Player.Get().m_investigatorCard.m_cardName, pc.m_cost));
	}

	public override bool CanPlayEvent()
	{
		return Player.Get().m_currentLocation.m_clues > 0;
	}
}
