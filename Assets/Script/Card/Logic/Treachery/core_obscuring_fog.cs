/********************************************************************
	created:	2018/06/07
	created:	7:6:2018   17:24
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_obscuring_fog : TreacheryLogic
{
	private UnityAction<int, Card> m_afterSkillTest;

	public override void OnReveal()
	{
		var tc = GetComponent<TreacheryCard>();
		var attachedCards = Player.Get().m_currentLocation.m_lstCardsAtHere;
		// Can only attach one at the mean time
		foreach(var attachment in attachedCards)
		{
			if(attachment.m_cardName == tc.m_cardName)
			{
				tc.Discard();
				GameLogic.Get().OutputGameLog("当前地点已有<朦胧迷雾>，丢弃摸到的\n");
				return;
			}
		}

		GameLogic.Get().SpawnAtLocation(tc, Player.Get().m_currentLocation, false);
		Player.Get().m_currentLocation.m_shroud += 2;

		m_afterSkillTest = new UnityAction<int, Card>(AfterSkillTest);
		GameLogic.Get().m_afterSkillTest.AddListener(m_afterSkillTest);
	}

	public void AfterSkillTest(int result, Card target)
	{
		if(Player.Get().GetCurrentAction() == PlayerAction.Investigate && result >= 0)
		{
			Player.Get().m_currentLocation.m_shroud -= 2;
			Player.Get().m_currentLocation.m_lstCardsAtHere.Remove(GetComponent<TreacheryCard>());
			GameLogic.Get().m_afterSkillTest.RemoveListener(m_afterSkillTest);
			GetComponent<TreacheryCard>().Discard();

			GameLogic.Get().OutputGameLog("<朦胧迷雾>被丢弃\n");
		}
	}
}
