/********************************************************************
	created:	2018/07/07
	created:	7:7:2018   8:07
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_manual_dexterity : PlayerCardLogic
{
	public override void OnSkillTest(int result)
	{
		if (result >= 0)
		{
			var card = GameLogic.Get().DrawPlayerCard();

			if(card != null)
			{
				Player.Get().AddHandCard(card);
				GameLogic.Get().OutputGameLog(string.Format("{0}因为<心灵手巧>获取1手牌<{1}>\n", Player.Get().m_investigatorCard.m_cardName, card.GetComponent<Card>().m_cardName));
			}
		}
	}
}
