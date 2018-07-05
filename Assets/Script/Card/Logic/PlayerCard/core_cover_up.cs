/********************************************************************
	created:	2018/07/05
	created:	5:7:2018   1:19
	author:		maval
	
	TODO:		1. The  reaction ability on this card is not Forced, so you may choose not to trigger it if you don't want to.
				2. When the game ends, if there are any clues on Cover Up: You suffer 1 mental trauma.
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_cover_up : PlayerCardLogic
{
	private int			m_cluesNeedResolve;
	private UnityAction	m_beforeGainClues;

	public override bool OnGainCard()
	{
		Player.Get().AddTreachery(GetComponent<PlayerCard>());
		m_cluesNeedResolve = 3;

		m_beforeGainClues = new UnityAction(BeforePlayerGainClues);
		GameLogic.Get().m_beforeGainClues.AddListener(m_beforeGainClues);

		return false;
	}

	public override string GetLog()
	{
		return string.Format("<掩盖>上线索数：<color=orange>{0}</color>", m_cluesNeedResolve);
	}

	private void BeforePlayerGainClues()
	{
		if(m_cluesNeedResolve == 0)
		{
			// Already resolve all clues.
			return;
		}

		UnityEngine.Assertions.Assert.IsTrue(Player.Get().m_cluesToGain > 0, "Assert failed in core_cover_up.BeforePlayerGainClues()!!!");

		// If you choose to discard a clue from Cover Up, the clue on your location remains intact. 
		int resolveClues = UnityEngine.Mathf.Min(Player.Get().m_cluesToGain, m_cluesNeedResolve);
		Player.Get().m_currentLocation.m_clues += resolveClues;

		GameLogic.Get().OutputGameLog(string.Format("{0}因为<掩盖>放回了当前地点的{1}点线索\n", Player.Get().m_investigatorCard.m_cardName, resolveClues));

		if (Player.Get().m_cluesToGain >= m_cluesNeedResolve)
		{
			Player.Get().m_cluesToGain -= m_cluesNeedResolve;
			m_cluesNeedResolve = 0;
		}
		else
		{
			m_cluesNeedResolve -= Player.Get().m_cluesToGain;
			Player.Get().m_cluesToGain = 0;
		}
	}
}
