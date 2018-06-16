/********************************************************************
	created:	2018/06/16
	created:	16:6:2018   11:19
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_beat_cop : PlayerCardLogic
{
	public override void OnReveal(Card card)
	{
		Player.Get().m_investigatorCard.m_combat += 1;
	}
}
