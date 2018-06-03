using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCard : Card
{
	public int	m_fight;
	public int	m_health;
	public int	m_evade;
	public int	m_damage;
	public int	m_horror;

	public bool				m_engaged { set; get; } = false;
	public PlayerCard		m_target { set; get; }
	public LocationCard		m_currentLocation { set; get; }

	public void HunterMoveToNearestInvestigator()
	{
		if(m_exhausted || m_engaged)
		{
			return;
		}

		UnityEngine.Assertions.Assert.IsNotNull(m_currentLocation, "A enemy is not at any place in HunterMoveToNearestInvestigator()!!");

		// If any investigator at the same location
		if(m_currentLocation.m_cardName == Player.Get().m_currentLocation.m_cardName)
		{
			Player.Get().AddEngagedEnemy(this);
			return;
		}

		throw new System.NotImplementedException();
	}

	public override void OnSkillTest()
	{
		GameLogic.Get().OutputGameLog(string.Format("{0}攻击了{1}！\n", Player.Get().m_investigatorCard.m_cardName, m_cardName));
		GameLogic.Get().m_currentScenario.m_skillTest.CombatTest(m_fight);
	}

	public override void OnSkillTestResult(int result)
	{
		if(result >= 0)
		{
			DecreaseHealth(1);
		}
		else
		{
			GameLogic.Get().OutputGameLog(string.Format("{0}未对{1}造成伤害...\n", Player.Get().m_investigatorCard.m_cardName, m_cardName));
		}
	}

	public void DecreaseHealth(int amount = 1)
	{
		m_health -= amount;
		GameLogic.Get().OutputGameLog(string.Format("{0}对{1}造成了{2}点伤害！\n", Player.Get().m_investigatorCard.m_cardName, m_cardName, amount));

		if(m_health <= 0)
		{
			m_engaged = false;
			GameLogic.m_lstUnengagedEnemyCards.Remove(this);
			Player.Get().RemoveEngagedEnemy(this);
			GameLogic.Get().m_lstDiscardEncounterCards.Add(gameObject);

			GameLogic.Get().OutputGameLog(string.Format("{0}被消灭了！\n", m_cardName));
		}
	}

	public override void OnSpawnAtLocation(LocationCard loc)
	{
		if(loc.m_cardName == Player.Get().m_currentLocation.m_cardName)
		{

		}
	}
}
