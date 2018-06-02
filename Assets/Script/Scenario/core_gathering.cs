using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class core_gathering : scenario_base
{
	void Awake()
	{
		InstantiateCards();
		GameLogic.Get().m_currentScenario = this;
	}

	void Start()
	{
		m_currentAct = Instantiate(m_lstActCards[0]).GetComponent<QuestCard>();
		m_currentAgenda = Instantiate(m_lstAgendaCards[0]).GetComponent<QuestCard>();

		GameLogic.DockCard(m_currentAct.gameObject, GameObject.Find("Act"));
		GameLogic.DockCard(m_currentAgenda.gameObject, GameObject.Find("Agenda"));

		m_startLocation = Instantiate(m_startLocation);

		GameLogic.DockCard(m_startLocation, GameObject.Find("StartLocation"));
		GameLogic.Get().PlayerEnterLocation(m_startLocation, false);

		m_lstOtherLocations.ForEach(location => { location.gameObject.SetActive(false); });

		string log = Player.Get().m_investigatorCard.m_cardName + "进入了场景。\n";
		GameLogic.Get().OutputGameLog(log);

		GameLogic.Get().m_mainGameUI.OnButtonEnterInvestigationPhase();
	}

	public override void ShowPlayInfo()
	{
		m_playerInfoText.text  = string.Format(
			"血量：<color=green>{0}</color>\n" +
			"神智：<color=green>{1}</color>\n" +
			"剩余行动：<color=green>{2}</color>\n" +
			"持有资源：<color=green>{3}</color>\n" +
			"持有线索：<color=green>{4}</color>\n" +
			"章节已推进标记数：<color=green>{5}</color>\n" +
			"恶兆已逼近标记数：<color=red>{6}</color>\n" +
			"各地点的线索：\n",
			Player.Get().GetHp(),
			Player.Get().GetSan(),
			Player.Get().ActionLeft(),
			Player.Get().m_resources,
			Player.Get().m_clues,
			m_currentAct.m_currentToken,
			m_currentAgenda.m_currentToken);

		m_revealedLocations.ForEach(loc => 
		{
			m_playerInfoText.text += string.Format("{0}： <color=orange>{1}</color>\n", loc.m_cardName, loc.m_clues);
		});
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
					GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "结算混沌标记：受到1点恐怖\n");
					break;
				default:
					break;

			}
		}
		else
		{
			throw new System.NotImplementedException("Hard/Expert difficulty not impl in AfterSkillTest()!!");
		}
	}

	public override void AdvanceAct()
	{
		++m_indexCurrentAct;

		if (m_indexCurrentAct >= m_lstActCards.Count)
		{

		}
		else
		{
			m_currentAct = Instantiate(m_lstActCards[m_indexCurrentAct]).GetComponent<QuestCard>();
			GameLogic.DockCard(m_currentAct.gameObject, GameObject.Find("Act"));

			if (m_indexCurrentAct == 1)
			{
				// Act 2
				m_startLocation.SetActive(false);
				m_revealedLocations.Remove(m_startLocation.GetComponent<LocationCard>());

				m_lstOtherLocations.ForEach(location => { location.gameObject.SetActive(true); });

				// Player start at hallway
				GameLogic.Get().PlayerEnterLocation(m_lstOtherLocations[0].gameObject);
			}
			else
			{
				// Act 3
			}
		}
	}

	public override void AdvanceAgenda()
	{
		++m_indexCurrentAgenda;

		if (m_indexCurrentAgenda >= m_lstAgendaCards.Count)
		{

		}
		else
		{
			m_currentAgenda= Instantiate(m_lstAgendaCards[m_indexCurrentAgenda]).GetComponent<QuestCard>();
			GameLogic.DockCard(m_currentAgenda.gameObject, GameObject.Find("Agenda"));

			if (m_indexCurrentAgenda == 1)
			{
				// Agenda 2
				List<string> options = new List<string>();
				options.Add("每位调查员受到2点恐怖");
				options.Add("每位调查员丢弃一张随机手牌");

				var mainUI = GameLogic.Get().m_mainGameUI;
				mainUI.m_choiceDropdown.ClearOptions();
				mainUI.m_choiceDropdown.AddOptions(options);
				mainUI.m_movementDropdown.RefreshShownValue();

				mainUI.m_choiceDropdown.gameObject.SetActive(true);
				mainUI.m_confirmChoiceBtn.gameObject.SetActive(true);
				mainUI.m_choiceMode = MainGame.ConfirmButtonMode.TextOnly;

				mainUI.m_lstChoiceEvent.Clear();
				mainUI.m_lstChoiceEvent.Add(new UnityEvent());
				mainUI.m_lstChoiceEvent.Add(new UnityEvent());

				mainUI.m_lstChoiceEvent[0].AddListener(new UnityAction(OnAgenda1_Option1));
				mainUI.m_lstChoiceEvent[1].AddListener(new UnityAction(OnAgenda1_Option2));
			}
			else
			{
				// Agenda 3

			}
		}
	}

	public void OnAgenda1_Option1()
	{
		Player.Get().DecreaseSanity(2);
		GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "选择了恶兆影响：受到2点恐怖\n");
		GameLogic.Get().m_mainGameUI.m_InvestigationPhaseBtn.gameObject.SetActive(true);
	}

	public void OnAgenda1_Option2()
	{
		var cards = Player.Get().GetHandCards();
		var cardToDiscard = cards[Random.Range(0, cards.Count-1)];

		GameLogic.Get().OutputGameLog(string.Format("{0}选择了恶兆影响：丢弃1张随机手牌<{1}>\n", Player.Get().m_investigatorCard.m_cardName, cardToDiscard.m_cardName));

		cardToDiscard.Discard();
		GameLogic.Get().m_mainGameUI.m_InvestigationPhaseBtn.gameObject.SetActive(true);
	}
}
