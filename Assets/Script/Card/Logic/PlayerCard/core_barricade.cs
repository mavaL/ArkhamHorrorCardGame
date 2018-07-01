/********************************************************************
	created:	2018/06/21
	created:	21:6:2018   8:30
	author:		maval
	
	TODO:       1. If an investigator that is engaged with an enemy moves to a Barricaded location, 
					the engaged enemy will disengage and remain in the investigator's previous 
					location (after making an attack of opportunity).	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_barricade : PlayerCardLogic
{
	private UnityAction<EnemyCard, LocationCard>	m_beforeEnemyMove;
	private LocationCard	m_attached;

	public override void OnReveal(Card card)
	{
		var pc = GetComponent<PlayerCard>();
		GameLogic.Get().OutputGameLog(string.Format("{0}打出<障碍物>，花费{1}资源\n", Player.Get().m_investigatorCard.m_cardName, pc.m_cost));

		m_attached = Player.Get().m_currentLocation;
		GameLogic.Get().SpawnAtLocation(pc, m_attached, false);

		m_beforeEnemyMove = new UnityAction<EnemyCard, LocationCard>(BeforeEnemyMove);
		GameLogic.Get().m_beforeEnemyMoveEvent.AddListener(m_beforeEnemyMove);

		Player.Get().m_currentAction.Pop();
	}

	private void BeforeEnemyMove(EnemyCard enemy, LocationCard dest)
	{
		if(dest.m_cardName == m_attached.m_cardName && !enemy.IsKeywordContain(Card.Keyword.Elite))
		{
			enemy.m_canMove = false;
			GameLogic.Get().OutputGameLog(string.Format("{0}被<障碍物>阻挡，无法进入{1}\n", enemy.m_cardName, dest.m_cardName));
		}
	}

	private void Update()
	{
		if(m_attached != null && m_attached.m_cardName != Player.Get().m_currentLocation.m_cardName)
		{
			GameLogic.Get().OutputGameLog(string.Format("{0}离开了{1}，丢弃<障碍物>\n", Player.Get().m_investigatorCard.m_cardName, m_attached.m_cardName));

			GameLogic.Get().m_beforeEnemyMoveEvent.RemoveListener(m_beforeEnemyMove);
			m_attached = null;
			GetComponent<PlayerCard>().Discard();
		}
	}
}