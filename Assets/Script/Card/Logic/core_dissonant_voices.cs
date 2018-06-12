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

public class core_dissonant_voices : Treachery
{
	private UnityAction					m_roundEnd;

	public override void OnReveal(TreacheryCard card)
	{
		Player.Get().AddTreachery(card);

		m_roundEnd = new UnityAction(OnRoundEnd);

		GameLogic.Get().m_mainGameUI.m_roundEndEvent.AddListener(m_roundEnd);
	}

	private void Update()
	{
		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_isActionEnable[PlayerAction.PlayCard] = false;
	}

	public void OnRoundEnd()
	{
		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_roundEndEvent.RemoveListener(m_roundEnd);
		GetComponent<TreacheryCard>().Discard();
		ui.m_isActionEnable[PlayerAction.PlayCard] = true;

		GameLogic.Get().OutputGameLog(string.Format("{0}摆脱了<不协和音>\n", Player.Get().m_investigatorCard.m_cardName));
	}
}
