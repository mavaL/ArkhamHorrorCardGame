/********************************************************************
	created:	2018/07/06
	created:	6:7:2018   0:56
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_guts : PlayerCardLogic
{
	public override void OnSkillTest(int result)
	{
		if (result >= 0)
		{
			var card = GameLogic.Get().DrawPlayerCard();

			if(card != null)
			{
				Player.Get().AddHandCard(card);
				GameLogic.Get().OutputGameLog(string.Format("{0}因为<勇气>获取1手牌<{1}>\n", Player.Get().m_investigatorCard.m_cardName, card.GetComponent<Card>().m_cardName));
			}
		}
	}
}
