/********************************************************************
	created:	2018/06/17
	created:	17:6:2018   15:05
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_lita_chantler: PlayerCardLogic
{
	public override void OnReveal(Card card)
	{
		Player.Get().m_investigatorCard.m_combat += 1;
		m_isActive = true;
	}

	public override void OnDiscard(Card card)
	{
		if(m_isActive)
		{
			m_isActive = false;
			Player.Get().m_investigatorCard.m_combat -= 1;
		}
	}
}
