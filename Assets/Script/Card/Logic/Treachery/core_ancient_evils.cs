/********************************************************************
	created:	2018/06/28
	created:	28:6:2018   22:44
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_ancient_evils : TreacheryLogic
{
	public override void OnReveal()
	{
		var agenda = GameLogic.Get().m_currentScenario.m_currentAgenda;

		if (agenda.AddDoom())
		{
			GameLogic.Get().OutputGameLog("恶兆已集满，触发事件！\n");
			GameLogic.Get().ShowHighlightCardExclusive(agenda, true);
			GameLogic.Get().m_mainGameUI.m_confirmAgendaResultBtn.gameObject.SetActive(true);
		}
	}
}
