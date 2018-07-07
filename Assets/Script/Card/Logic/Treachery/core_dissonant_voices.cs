/********************************************************************
	created:	2018/06/07
	created:	7:6:2018   12:58
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_dissonant_voices : TreacheryLogic
{
	private UnityAction					m_roundEnd;

	public override void OnReveal()
	{
		Player.Get().AddTreachery(GetComponent<TreacheryCard>());

		m_roundEnd = new UnityAction(OnRoundEnd);

		GameLogic.Get().m_mainGameUI.m_roundEndEvent.AddListener(m_roundEnd);

		m_isActive = true;
	}

	private void Update()
	{
		if(m_isActive)
		{
			var ui = GameLogic.Get().m_mainGameUI;
			ui.m_isActionEnable[PlayerAction.PlayCard] = false;
		}
	}

	private void OnRoundEnd()
	{
		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_roundEndEvent.RemoveListener(m_roundEnd);

		GetComponent<TreacheryCard>().Discard();

		ui.m_isActionEnable[PlayerAction.PlayCard] = true;
		m_isActive = false;

		GameLogic.Get().OutputGameLog(string.Format("{0}摆脱了<不协和音>\n", Player.Get().m_investigatorCard.m_cardName));
	}
}
