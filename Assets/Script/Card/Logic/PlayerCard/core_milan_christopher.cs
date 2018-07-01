/********************************************************************
	created:	2018/06/27
	created:	27:6:2018   8:23
	author:		maval
	
	TODO:	    1. You can successfully Investigate a location even if there are no clues on it.
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_milan_christopher : PlayerCardLogic
{
	private UnityAction<int, Card> m_afterSkillTest;

	public override void OnReveal(Card card)
	{
		Player.Get().m_investigatorCard.m_intellect += 1;

		m_afterSkillTest = new UnityAction<int, Card>(AfterSkillTest);
		GameLogic.Get().m_afterSkillTest.AddListener(m_afterSkillTest);

		m_isActive = true;
	}

	public void AfterSkillTest(int result, Card target)
	{
		if (Player.Get().GetCurrentAction() == PlayerAction.Investigate && result >= 0)
		{
			Player.Get().m_resources += 1;
			GameLogic.Get().OutputGameLog("<米兰.克里斯托弗博士>使得调查中获得1资源\n");
		}
	}

	public override void OnDiscard(Card card)
	{
		if(m_isActive)
		{
			m_isActive = false;

			GameLogic.Get().m_afterSkillTest.RemoveListener(m_afterSkillTest);
			Player.Get().m_investigatorCard.m_intellect -= 1;
		}
	}
}
