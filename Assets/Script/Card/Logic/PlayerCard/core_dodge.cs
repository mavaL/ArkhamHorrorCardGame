/********************************************************************
	created:	2018/06/12
	created:	12:6:2018   0:10
	author:		maval
	
	TODO:		1. Attack of opportunity 
				2. Retaliate attack
				3. Maybe better to show two highlight cards, left is enemy, right is reactive event
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_dodge : PlayerCardLogic
{
	public override void OnReveal(Card card)
	{
		card.OnExhausted();
		var pc = GetComponent<PlayerCard>();

		GameLogic.Get().OutputGameLog(string.Format("{0}对{1}打出<闪避>，花费{2}资源，未受到伤害\n", Player.Get().m_investigatorCard.m_cardName, card.m_cardName, pc.m_cost));
	}
}
