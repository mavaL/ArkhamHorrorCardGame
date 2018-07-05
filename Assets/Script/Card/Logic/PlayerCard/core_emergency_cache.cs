/********************************************************************
	created:	2018/07/05
	created:	5:7:2018   23:58
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_emergency_cache : PlayerCardLogic
{
	public override void OnReveal(Card card)
	{
		var pc = GetComponent<PlayerCard>();
		GameLogic.Get().OutputGameLog(string.Format("{0}打出<应急藏匿处>，获得3资源\n", Player.Get().m_investigatorCard.m_cardName));

		Player.Get().m_resources += 3;

		Player.Get().m_currentAction.Pop();
	}
}