/********************************************************************
	created:	2018/06/21
	created:	21:6:2018   8:30
	author:		maval
	
	TODO:       1. If an investigator that is engaged with an enemy moves to a Barricaded location, 
					the engaged enemy will disengage and remain in the investigator's previous 
					location (after making an attack of opportunity).	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_barricade : PlayerCardLogic
{
	public override void OnReveal(Card card)
	{
		var pc = GetComponent<PlayerCard>();
		GameLogic.Get().SpawnAtLocation(pc, Player.Get().m_currentLocation, false);

		GameLogic.Get().OutputGameLog(string.Format("{0}打出<障碍物>，花费{1}资源\n", Player.Get().m_investigatorCard.m_cardName, pc.m_cost));
	}
}