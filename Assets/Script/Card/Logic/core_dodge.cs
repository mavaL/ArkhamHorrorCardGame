/********************************************************************
	created:	2018/06/12
	created:	12:6:2018   0:10
	author:		maval
	
	TODO:		1. Attack of opportunity 
				2. Retaliate attack
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
		GetComponent<PlayerCard>().Discard();
		GameLogic.Get().OutputGameLog(string.Format("{0}对{1}打出闪避，未受到伤害\n", Player.Get().m_investigatorCard.m_cardName, card.m_cardName));
	}
}
