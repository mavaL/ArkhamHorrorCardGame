using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class core_devourer_below : scenario_base
{
	void Awake()
	{
		GameLogic.Get().m_currentScenario = this;
	}

	void Start()
	{
		GameLogic.Get().m_currentPhase = TurnPhase.InvestigationPhase;

		m_currentAct = Instantiate(m_lstActCards[0]).GetComponent<QuestCard>();
		m_currentAgenda = Instantiate(m_lstAgendaCards[0]).GetComponent<QuestCard>();

		GameLogic.DockCard(m_currentAct.gameObject, GameObject.Find("Act"));
		GameLogic.DockCard(m_currentAgenda.gameObject, GameObject.Find("Agenda"));

		m_startLocation = Instantiate(m_startLocation);

		GameLogic.DockCard(m_startLocation, GameObject.Find("StartLocation"));
		GameLogic.Get().PlayerEnterLocation(m_startLocation);

		m_revealedLocations.Add(m_startLocation);

		string log = Player.Get().m_investigatorCard.m_cardName + "进入了场景。\n";
		GameLogic.Get().OutputGameLog(log);
	}

	public override void ShowPlayInfo()
	{
		LocationCard card = m_revealedLocations[0].GetComponent<LocationCard>();

		var textComp = GameObject.Find("ClueInfo").GetComponent<Text>();
		textComp.text  = string.Format(
			"剩余行动：<color=green>{0}</color>\n" +
			"持有资源：<color=green>{1}</color>\n" +
			"持有线索：<color=green>{2}</color>\n" +
			"章节已推进标记数：<color=green>{3}</color>\n" +
			"恶兆已逼近标记数：<color=red>{4}</color>\n" +
			"各地点的线索：\n" +
			"书房： <color=orange>{5}</color>\n", 
			3 - Player.Get().m_actionUsed,
			Player.Get().m_resources,
			Player.Get().m_clues,
			m_currentAct.m_currentToken,
			m_currentAgenda.m_currentToken,
			card.m_clues);
	}

	public override int GetChaosTokenEffect(ChaosBag.ChaosTokenType t)
	{
		if(GameLogic.Get().m_difficulty == GameDifficulty.Easy ||
		GameLogic.Get().m_difficulty == GameDifficulty.Normal )
		{
			switch (t)
			{
				case ChaosBag.ChaosTokenType.Skully:
					return 0 - Card.HowManyEnemyCardContainTheKeyword(Player.Get().m_currentLocation.m_lstEnemies, Card.Keyword.Ghoul);
				case ChaosBag.ChaosTokenType.Cultist:
					return -1;
				case ChaosBag.ChaosTokenType.Tablet:
					if (Card.HowManyEnemyCardContainTheKeyword(Player.Get().m_currentLocation.m_lstEnemies, Card.Keyword.Ghoul) > 0)
					{
						Player.Get().DecreaseHealth();
						GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "结算混沌标记：受到1点伤害\n");
					}
					return -2;
                default:
					break;
			}
		}
		else
		{
			Debug.Log("Hard/Expert difficulty not impl in GetChaosTokenEffect()!!");
		}
		
		Assert.IsTrue(false, "Assert failed in GetChaosTokenEffect()!!");
		return -1;
	}

	public override void AfterSkillTestFailed(ChaosBag.ChaosTokenType t)
	{
		if (GameLogic.Get().m_difficulty == GameDifficulty.Easy ||
		GameLogic.Get().m_difficulty == GameDifficulty.Normal)
		{
			switch (t)
			{
				case ChaosBag.ChaosTokenType.Cultist:
					Player.Get().DecreaseSanity(1);
					GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "结算混沌标记：神智减1\n");
					break;
				default:
					break;

			}
		}
		else
		{
			Debug.Log("Hard/Expert difficulty not impl in AfterSkillTest()!!");
		}
	}

	public override void AdvanceAct()
	{
		throw new System.NotImplementedException();
	}
}
