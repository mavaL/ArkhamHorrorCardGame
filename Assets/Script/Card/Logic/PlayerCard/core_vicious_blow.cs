/********************************************************************
	created:	2018/06/26
	created:	26:6:2018   22:43
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_vicious_blow : PlayerCardLogic
{
	public override void OnSkillTest(int result)
	{
		if (Player.Get().GetCurrentAction() != PlayerAction.Fight || result < 0)
		{
			return;
		}

		Player.Get().m_attackDamage += 1;
		m_isActive = true;
		GameLogic.Get().OutputGameLog(string.Format("{0}打出<残忍打击>，本次攻击伤害+1\n", Player.Get().m_investigatorCard.m_cardName));
	}

	public override void OnDiscard(Card card)
	{
		if(m_isActive)
		{
			Player.Get().m_attackDamage -= 1;
			m_isActive = false;
		}
	}
}
