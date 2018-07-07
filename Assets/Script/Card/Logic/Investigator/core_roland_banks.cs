/********************************************************************
	created:	2018/07/04
	created:	4:7:2018   20:10
	author:		maval
	
	TODO:		1. Player may decide to use roland's ability or not.	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_roland_banks : InvestigatorLogic
{
	private UnityAction<EnemyCard> m_afterEnemyDamaged;
	private UnityAction m_roundEnd;
	private bool m_isAbilityUsed = false;

	public override void EnterMainGame()
	{
		m_afterEnemyDamaged = new UnityAction<EnemyCard>(AfterEnemyDamaged);
		m_roundEnd = new UnityAction(OnRoundEnd);

		GameLogic.Get().m_afterEnemyDamagedEvent.AddListener(m_afterEnemyDamaged);
		GameLogic.Get().m_mainGameUI.m_roundEndEvent.AddListener(m_roundEnd);
	}

	private void AfterEnemyDamaged(EnemyCard target)
	{
		if(!m_isAbilityUsed && target.m_health <= 0 && Player.Get().m_currentLocation.m_clues > 0)
		{
			GameLogic.Get().OutputGameLog(string.Format("{0}通过自身能力，获得了1线索\n", GetComponent<InvestigatorCard>().m_cardName));

			Player.Get().GainClues(1);
			Player.Get().m_currentLocation.m_clues -= 1;

			m_isAbilityUsed = true;
		}
	}

	private void OnRoundEnd()
	{
		m_isAbilityUsed = false;
	}

	public override int OnApplyElderSignEffect()
	{
		return Player.Get().m_currentLocation.m_clues;
	}
}
