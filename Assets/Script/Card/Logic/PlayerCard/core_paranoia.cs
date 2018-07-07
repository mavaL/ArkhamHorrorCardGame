/********************************************************************
	created:	2018/07/07
	created:	7:7:2018   9:21
	author:		maval
	
	TODO:		1. When reveal at upkeep phase, should be highlighted	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_paranoia : PlayerCardLogic
{
	public override bool OnGainCard()
	{
		GameLogic.Get().OutputGameLog(string.Format("{0}因为<偏执狂>失去了全部{1}点资源！\n", Player.Get().m_investigatorCard.m_cardName, Player.Get().m_resources));

		Player.Get().m_resources = 0;

		return false;
	}
}
