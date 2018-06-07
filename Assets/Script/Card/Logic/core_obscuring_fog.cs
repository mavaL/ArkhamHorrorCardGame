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

public class core_obscuring_fog : Treachery
{
	private UnityAction<int>	m_onAttachLocationInvestigate;

	public override void OnReveal(TreacheryCard card)
	{
		var attachedCards = Player.Get().m_currentLocation.m_lstCardsAtHere;
		// Can only attach one at the mean time
		foreach(var attachment in attachedCards)
		{
			if(attachment.m_cardName == card.m_cardName)
			{
				card.Discard();
				GameLogic.Get().OutputGameLog("当前地点已有<朦胧迷雾>，丢弃摸到的\n");
				return;
			}
		}

		GameLogic.Get().SpawnAtLocation(card, Player.Get().m_currentLocation, false);
		attachedCards.Add(card);
		Player.Get().m_currentLocation.m_shroud += 2;

		m_onAttachLocationInvestigate = new UnityAction<int>(OnAttachLocationInvestigate);
		Player.Get().m_currentLocation.m_onLocationInvestigate.AddListener(m_onAttachLocationInvestigate);
	}

	public void OnAttachLocationInvestigate(int result)
	{
		if(result >= 0)
		{
			Player.Get().m_currentLocation.m_shroud -= 2;
			Player.Get().m_currentLocation.m_lstCardsAtHere.Remove(GetComponent<TreacheryCard>());
			Player.Get().m_currentLocation.m_onLocationInvestigate.RemoveListener(m_onAttachLocationInvestigate);
			GetComponent<TreacheryCard>().Discard();

			GameLogic.Get().OutputGameLog("<朦胧迷雾>被丢弃\n");
		}
	}
}
