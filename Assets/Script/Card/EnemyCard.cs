using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCard : Card
{
	public int				m_fight;
	public int				m_health;
	public int				m_evade;
	public int				m_damage;
	public int				m_horror;
	public LocationCard		m_spawnLocation;

	public bool				m_engaged { set; get; } = false;
	public PlayerCard		m_target { set; get; }

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

		// TODO: hunter enemy pathfinding..
		throw new System.NotImplementedException();
	}

	public override void OnSkillTest()
	{
		if(Player.Get().m_currentAction.Peek() == PlayerAction.Fight)
		{
			GameLogic.Get().m_currentScenario.m_skillTest.CombatTest(m_fight);
		}
		else
		{
			GameLogic.Get().m_currentScenario.m_skillTest.AgilityTest(m_evade);
		}
	}

	public override void OnSkillTestResult(int result)
	{
		//result = 99;
		if (result >= 0)
		{
			if (Player.Get().m_currentAction.Peek() == PlayerAction.Fight)
			{
				GameLogic.Get().m_beforeEnemyDamagedEvent.Invoke(this);

				DecreaseHealth(Player.Get().m_attackDamage);
			}
			else
			{
				Player.Get().RemoveEngagedEnemy(this);
				GameLogic.m_lstUnengagedEnemyCards.Add(this);
				GameLogic.DockCard(gameObject, Player.Get().m_currentLocation.gameObject, 300, true, true);

				OnExhausted();

				GameLogic.Get().OutputGameLog("闪避结果成功\n");
			}
		}
		else
		{
			if(Player.Get().m_currentAction.Peek() == PlayerAction.Fight)
			{
				GameLogic.Get().OutputGameLog("结果未造成伤害\n");
			}
			else
			{
				GameLogic.Get().OutputGameLog("闪避结果失败\n");
			}
		}
		Player.Get().ActionDone(Player.Get().m_currentAction.Peek());
	}

	public void DecreaseHealth(int amount = 1)
	{
		StartCoroutine(_DecreaseHealth(amount));
	}

	private IEnumerator _DecreaseHealth(int amount)
	{
		m_health -= amount;
		GameLogic.Get().OutputGameLog(string.Format("{0}对{1}造成了{2}点伤害！\n", Player.Get().m_investigatorCard.m_cardName, m_cardName, amount));

		GameLogic.Get().m_afterEnemyDamagedEvent.Invoke();

		if (m_health <= 0)
		{		
			GameLogic.Get().OutputGameLog(string.Format("{0}被消灭了！\n", m_cardName));

			if (GameLogic.Get().m_mainGameUI.OnEventTiming(EventTiming.DefeatEnemy))
			{
				GameLogic.Get().ShowHighlightCardExclusive(this, false, false);
				yield return new WaitUntil(() => GameLogic.Get().m_currentTiming == EventTiming.None);
			}
	
			Discard();
		}
		else
		{
			Card.m_lstSelectCards.ForEach(card =>
			{
				PlayerCard pc = card as PlayerCard;
				pc.m_skillCardEffect.Invoke(99, gameObject);
			});
		}
	}

	public override void OnSpawnAtLocation(LocationCard loc)
	{
		GameLogic.Get().OutputGameLog(string.Format("<{0}>出现在了<{1}>\n", m_cardName, loc.m_cardName));

		if (loc.m_cardName == Player.Get().m_currentLocation.m_cardName)
		{
			Player.Get().AddEngagedEnemy(this);
		}
		else
		{
			GameLogic.m_lstUnengagedEnemyCards.Add(this);
		}
	}

	public override void OnRecoverFromExhaust()
	{
		m_exhausted = false;
		m_thisInListView.gameObject.transform.Rotate(0, 0, -90);

		if(m_currentLocation == Player.Get().m_currentLocation)
		{
			Player.Get().AddEngagedEnemy(this);
		}
	}

	public override void OnExhausted()
	{
		if(m_health > 0)
		{
			m_exhausted = true;
			GameLogic.m_lstExhaustedCards.Add(this);
			m_thisInListView.gameObject.transform.Rotate(0, 0, 90);
		}	
	}

	public override void Discard(bool bFromAssetArea = false)
	{
		base.Discard(bFromAssetArea);

		GameLogic.m_lstUnengagedEnemyCards.Remove(this);

		if(m_engaged)
		{
			Player.Get().RemoveEngagedEnemy(this);
		}
		else
		{
			m_currentLocation.m_lstCardsAtHere.Remove(this);
			m_currentLocation = null;
		}

		GameLogic.Get().m_lstDiscardEncounterCards.Add(gameObject);
		gameObject.SetActive(false);
	}
}
