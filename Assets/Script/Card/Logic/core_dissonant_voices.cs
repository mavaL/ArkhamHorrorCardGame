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
	private UnityAction<PlayerAction>	m_actionDone;
	private UnityAction					m_roundEnd;

	public override void OnReveal(TreacheryCard card)
	{
		Player.Get().AddTreachery(card);

		m_actionDone = new UnityAction<PlayerAction>(OnActionDone);
		m_roundEnd = new UnityAction(OnRoundEnd);

		Player.Get().m_actionDoneEvent.AddListener(m_actionDone);
		GameLogic.Get().m_mainGameUI.m_roundEndEvent.AddListener(m_roundEnd);
	}

	public void OnActionDone(PlayerAction action)
	{
		if(action == PlayerAction.Move || action == PlayerAction.Fight || action == PlayerAction.Evade)
		{
			if(!Player.Get().m_actionRecord.Contains(action))
			{
				Player.Get().m_actionUsed += 1;
				GameLogic.Get().OutputGameLog(string.Format("{0}受<魂飞魄散>的效果，多消耗了1点行动\n", Player.Get().m_investigatorCard.m_cardName));
			}
		}
		
	}

	private void Update()
	{
		var ui = GameLogic.Get().m_mainGameUI;

		
	}

	public void OnRoundEnd()
	{
		Player.Get().m_actionDoneEvent.RemoveListener(m_actionDone);
		GameLogic.Get().m_mainGameUI.m_roundEndEvent.RemoveListener(m_roundEnd);
		GetComponent<TreacheryCard>().Discard();

		GameLogic.Get().OutputGameLog(string.Format("{0}摆脱了<不协和音>\n", Player.Get().m_investigatorCard.m_cardName));
	}
}
