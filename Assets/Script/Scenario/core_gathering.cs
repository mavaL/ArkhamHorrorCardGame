using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class core_gathering : scenario_base
{
	public AllyCard		m_lita;
	public EnemyCard	m_ghoulPriest;
	private UnityAction	m_onParleyConfirm;
	private UnityAction<int>	m_onActionDropdownChanged;
	private PlayerAction		m_parleyAction;

	void Awake()
	{
		InstantiateCards();

		m_lita = Instantiate(m_lita.gameObject).GetComponent<AllyCard>();
		m_lita.m_isPlayerDeck = false;

		m_ghoulPriest = Instantiate(m_ghoulPriest.gameObject).GetComponent<EnemyCard>();

		GameLogic.Get().m_currentScenario = this;
	}

	void Start()
	{
		m_currentAct = Instantiate(m_lstActCards[0]).GetComponent<QuestCard>();
		m_currentAgenda = Instantiate(m_lstAgendaCards[0]).GetComponent<QuestCard>();

		GameLogic.DockCard(m_currentAct.gameObject, GameObject.Find("Act"));
		GameLogic.DockCard(m_currentAgenda.gameObject, GameObject.Find("Agenda"));

		m_startLocation = Instantiate(m_startLocation);
		GameLogic.Get().PlayerEnterLocation(m_startLocation, false);
		GameLogic.DockCard(m_startLocation, GameObject.Find("StartLocation"));

		m_lstOtherLocations.ForEach(location => { location.gameObject.SetActive(false); });

		GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "进入了场景\n章节1开始\n");

		m_onParleyConfirm = new UnityAction(OnButtonParleyConfirm);
		m_onActionDropdownChanged = new UnityAction<int>(OnActionDropdownChange);

		GameLogic.Get().m_mainGameUI.OnButtonEnterInvestigationPhase();
	}

	public override void ShowPlayInfo()
	{
		var ui = GameLogic.Get().m_mainGameUI;

		ui.m_statsInfoText.text  = string.Format(
			"血量：<color=green>{0}</color> 神智：<color=green>{1}</color> 意志：<color=green>{2}</color> 知识：<color=green>{3}</color> 力量：<color=green>{4}</color> 敏捷：<color=green>{5}</color>\n" +
			"剩余行动：<color=green>{6}</color>\n" +
			"持有资源：<color=green>{7}</color>\n" +
			"持有线索：<color=green>{8}</color>\n" +
			"章节已推进标记数：<color=green>{9}</color>\n" +
			"恶兆已逼近标记数：<color=red>{10}</color>\n" +
			"各地点的线索：\n",
			Player.Get().GetHp(),
			Player.Get().GetSan(),
			Player.Get().m_investigatorCard.m_willPower,
			Player.Get().m_investigatorCard.m_intellect,
			Player.Get().m_investigatorCard.m_combat,
			Player.Get().m_investigatorCard.m_agility,
			Player.Get().ActionLeft(),
			Player.Get().m_resources,
			Player.Get().m_clues,
			m_currentAct.m_currentToken,
			m_currentAgenda.m_currentToken);

		m_revealedLocations.ForEach(loc => 
		{
			ui.m_statsInfoText.text += string.Format("{0}：<color=orange>{1}</color>\n", loc.m_cardName, loc.m_clues);
		});

		ui.m_statsInfoText.text += "资产区统计：\n";

		var ally = Player.Get().GetAssetCardInSlot(AssetSlot.Ally) as AllyCard;
		if(ally != null)
		{
			ui.m_statsInfoText.text += string.Format("{0}\n血量：<color=green>{1}</color> 神智：<color=green>{2}</color>\n", ally.m_cardName, ally.m_health, ally.m_sanity);
		}

		var leftHandAsset = Player.Get().GetAssetCardInSlot(AssetSlot.Hand1);
		if (leftHandAsset != null && leftHandAsset.GetComponent<PlayerCardLogic>())
		{
			if(leftHandAsset.GetComponent<PlayerCardLogic>().HasUseLimit())
			{
				ui.m_statsInfoText.text += string.Format("{0}\n剩余使用次数：<color=green>{1}</color>\n", leftHandAsset.m_cardName, leftHandAsset.GetComponent<PlayerCardLogic>().GetAssetResource());
			}
		}

		ui.m_statsInfoText.text += "威胁区统计：\n";

		var enemies = Player.Get().GetEnemyCards();
		enemies.ForEach(enemy =>
		{
			ui.m_statsInfoText.text += string.Format("{0}血量：<color=red>{1}</color>\n", enemy.m_cardName, enemy.m_health);
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
					return 0 - Card.HowManyEnemyCardContainTheKeyword(Player.Get().m_currentLocation.m_lstCardsAtHere, Card.Keyword.Ghoul);
				case ChaosBag.ChaosTokenType.Cultist:
					return -1;
				case ChaosBag.ChaosTokenType.Tablet:
					if (Card.HowManyEnemyCardContainTheKeyword(Player.Get().m_currentLocation.m_lstCardsAtHere, Card.Keyword.Ghoul) > 0)
					{
						Player.Get().DecreaseHealth(null, 1);
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
			// Reach the end
			List<string> options = new List<string>();
			options.Add("烧家!");
			options.Add("不烧家...");

			var mainUI = GameLogic.Get().m_mainGameUI;
			mainUI.m_choiceDropdown.ClearOptions();
			mainUI.m_choiceDropdown.AddOptions(options);
			mainUI.m_choiceDropdown.RefreshShownValue();

			mainUI.m_choiceDropdown.gameObject.SetActive(true);
			mainUI.m_confirmChoiceBtn.gameObject.SetActive(true);
			mainUI.m_choiceMode = MainGame.ConfirmButtonMode.TextOnly;

			mainUI.m_lstChoiceEvent.Clear();
			mainUI.m_lstChoiceEvent.Add(new UnityEvent());
			mainUI.m_lstChoiceEvent.Add(new UnityEvent());

			mainUI.m_lstChoiceEvent[0].AddListener(new UnityAction(OnAct_Ending1));
			mainUI.m_lstChoiceEvent[1].AddListener(new UnityAction(OnAct_Ending2));
		}
		else
		{
			m_currentAct = Instantiate(m_lstActCards[m_indexCurrentAct]).GetComponent<QuestCard>();
			GameLogic.DockCard(m_currentAct.gameObject, GameObject.Find("Act"));

			if (m_indexCurrentAct == 1)
			{
				// Act 2
				GameLogic.Get().OutputGameLog("章节2开始\n");

				m_startLocation.GetComponent<Card>().Discard();
				m_revealedLocations.Remove(m_startLocation.GetComponent<LocationCard>());

				m_lstOtherLocations.ForEach(location => { location.gameObject.SetActive(true); });

				// Player start at hallway
				GameLogic.Get().PlayerEnterLocation(m_lstOtherLocations[0].gameObject, false);
			}
			else
			{
				// Act 3
				GameLogic.Get().OutputGameLog("章节3开始\n");
				StartCoroutine(Act3Setup());
			}
		}
	}

	IEnumerator Act3Setup()
	{
		GameLogic.Get().RevealLocation(m_lstOtherLocations[3]);

		m_lstOtherLocations[0].m_lstDestinations.Add(m_lstOtherLocations[3]);

		var ui = GameLogic.Get().m_mainGameUI;

		yield return new WaitUntil(() => ui.m_bConfirmModeEnd == true);

		GameLogic.Get().SpawnAtLocation(m_lita, m_lstOtherLocations[3], true);
		yield return new WaitUntil(() => ui.m_bConfirmModeEnd == true);

		GameLogic.Get().SpawnAtLocation(m_ghoulPriest, m_lstOtherLocations[0], true);

		ui.m_actionDropdown.options.Add(new Dropdown.OptionData("与丽塔谈判"));
		m_parleyAction = (PlayerAction)ui.m_actionDropdown.options.Count - 1;
		ui.m_actionDropdown.onValueChanged.AddListener(m_onActionDropdownChanged);
	}

	public void OnActionDropdownChange(int index)
	{
		if(index == (int)m_parleyAction)
		{
			Player.Get().m_currentAction = m_parleyAction;

			var ui = GameLogic.Get().m_mainGameUI;
			ui.m_confirmChoiceBtn.gameObject.SetActive(true);
			ui.m_choiceMode = MainGame.ConfirmButtonMode.ParleyWithLita;
			ui.m_confirmChoiceBtn.onClick.AddListener(m_onParleyConfirm);

			ui.BeginSelectCardToSpend();
			Player.Get().ActionDone(m_parleyAction);
		}
	}

	public override void AdvanceAgenda()
	{
		++m_indexCurrentAgenda;

		if (m_indexCurrentAgenda >= m_lstAgendaCards.Count)
		{
			if(m_indexCurrentAct < 2)
			{
				_ShowEnding(3);
			}
			else
			{
				_ShowEnding(4);
			}
		}
		else
		{
			m_currentAgenda= Instantiate(m_lstAgendaCards[m_indexCurrentAgenda]).GetComponent<QuestCard>();
			GameLogic.DockCard(m_currentAgenda.gameObject, GameObject.Find("Agenda"));

			if (m_indexCurrentAgenda == 1)
			{
				// Effect of agenda 1
				List<string> options = new List<string>();
				options.Add("每位调查员受到2点恐怖");
				options.Add("每位调查员丢弃一张随机手牌");

				var mainUI = GameLogic.Get().m_mainGameUI;
				mainUI.m_choiceDropdown.ClearOptions();
				mainUI.m_choiceDropdown.AddOptions(options);
				mainUI.m_choiceDropdown.RefreshShownValue();

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
				// Effect of agenda 2
				m_lstEncounterCards.AddRange(GameLogic.Get().m_lstDiscardEncounterCards);
				GameLogic.Shuffle(m_lstEncounterCards);
				GameLogic.Get().m_lstDiscardEncounterCards.Clear();

				GameLogic.Get().ShowHighlightCardExclusive(m_lstEncounterCards[0].GetComponent<Card>(), false);

				var mainUI = GameLogic.Get().m_mainGameUI;
				mainUI.m_choiceMode = MainGame.ConfirmButtonMode.DrawEncounterCard;
				mainUI.m_confirmChoiceBtn.gameObject.SetActive(true);
			}
		}
	}

	private void OnAgenda1_Option1()
	{
		Player.Get().DecreaseSanity(2);
		GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "选择了恶兆影响：受到2点恐怖\n");
		GameLogic.Get().m_mainGameUI.m_InvestigationPhaseBtn.gameObject.SetActive(true);
	}

	private void OnAgenda1_Option2()
	{
		var cards = Player.Get().GetHandCards();
		var cardToDiscard = cards[Random.Range(0, cards.Count-1)];

		GameLogic.Get().OutputGameLog(string.Format("{0}选择了恶兆影响：丢弃1张随机手牌<{1}>\n", Player.Get().m_investigatorCard.m_cardName, cardToDiscard.m_cardName));

		cardToDiscard.Discard();
		GameLogic.Get().m_mainGameUI.m_InvestigationPhaseBtn.gameObject.SetActive(true);
	}

	private void OnAct_Ending1()
	{
		_ShowEnding(1);
	}

	private void OnAct_Ending2()
	{
		_ShowEnding(2);
	}

	private void _ShowEnding(int which)
	{
		var mainUI = GameLogic.Get().m_mainGameUI;
		mainUI.m_lstChoiceEvent.Clear();
		mainUI.m_endingPanel.SetActive(true);

		if(which == 1)
		{
			mainUI.m_endingPanel.GetComponentInChildren<Text>().text = "你点点头，允许那个红发女人将你屋子的墙壁和地板逐一点燃。火焰燃烧的很快，你快速冲出前门，以免又被抓回地狱。你站在人行道上，看着你所拥有的一切被火焰所吞没。“跟我来。”那个女人说道，“你必须知道我们将要面对什么。如果我们孤军奋战，我们必定失败……但如果我们并肩作战，我们就能阻止这一切。”";
		}
		else if(which == 2)
		{
			mainUI.m_endingPanel.GetComponentInChildren<Text>().text = "你拒绝服从那个过分热心女人的建议，你将她推出你的屋子，以免她在没有获取你允许的情况下点燃整个屋子。“蠢货！你犯了一个很严重的错误！”她警告道，“你根本不知道那些凶兆之下潜伏着什么……你害我们陷入了巨大的危机之中！”经历了今晚的事情，你仍在瑟瑟发抖，你决定让这个女人说完。或许她能在这些离奇诡异的事件中指出一条明路……但似乎她并不怎么信任你。";
		}
		else if (which == 3)
		{
			mainUI.m_endingPanel.GetComponentInChildren<Text>().text = "你跑向客厅，试图找到一条可以逃出屋子的路。但是炽热的障碍依旧挡住了你前进的道路。你完全陷入困境，一大群令人恐惧的生物涌入你的屋子并将你包围，而你无路可逃。";
		}
		else
		{
			mainUI.m_endingPanel.GetComponentInChildren<Text>().text = "Game Over";
		}

		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = false;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = false;

		mainUI.m_confirmToMainMenuBtn.gameObject.SetActive(true);
	}

	void Update()
	{
		var ui = GameLogic.Get().m_mainGameUI;

		if(m_indexCurrentAct == 1)
		{
			if (GameLogic.Get().m_currentPhase == TurnPhase.UpkeepPhase &&
				Player.Get().m_currentLocation == m_lstOtherLocations[0] &&
				GameLogic.Get().IsClueEnoughToAdvanceAct())
			{
				ui.m_advanceActBtn.gameObject.SetActive(true);
			}
			else
			{
				ui.m_advanceActBtn.gameObject.SetActive(false);
			}
		}
		else if(m_indexCurrentAct == 2)
		{
			if(GameLogic.Get().m_currentPhase == TurnPhase.InvestigationPhase &&
				!m_lita.m_hasOwner &&
				m_lita.m_currentLocation &&
				Player.Get().m_currentLocation.m_cardName == m_lita.m_currentLocation.m_cardName)
			{
				ui.m_isActionEnable[m_parleyAction] = true;
			}
			else
			{
				ui.m_isActionEnable[m_parleyAction] = false;
			}

			for(int i=0; i<GameLogic.Get().m_lstDiscardEncounterCards.Count; ++i)
			{
				if(GameLogic.Get().m_lstDiscardEncounterCards[i].GetComponent<Card>().m_cardName == m_ghoulPriest.m_cardName)
				{
					ui.m_advanceActBtn.gameObject.SetActive(true);
					break;
				}
			}
		}
	}

	public void OnButtonParleyConfirm()
	{
		GameLogic.Get().OutputGameLog(string.Format("{0}执行行动与丽塔谈判\n", Player.Get().m_investigatorCard.m_cardName));

		var ui = GameLogic.Get().m_mainGameUI;
		UnityEngine.Assertions.Assert.AreEqual(MainGame.ConfirmButtonMode.ParleyWithLita, ui.m_choiceMode, "Assert failed in OnButtonParleyConfirm()!!");

		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = true;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = true;

		ui.m_confirmChoiceBtn.gameObject.SetActive(false);
		ui.m_confirmChoiceBtn.onClick.RemoveListener(m_onParleyConfirm);
		ui.m_actionDropdown.options.RemoveAt((int)m_parleyAction);
		Player.Get().m_currentAction = PlayerAction.None;

		ChaosBag.ChaosTokenType chaosToken;
		int result = GameLogic.Get().SkillTest(SkillType.Intellect, 4, out chaosToken);
		bool bSucceed = result >= 0;
		bSucceed = true;

		if (bSucceed)
		{
			Player.Get().AddAssetCard(m_lita, false);
			m_lita.m_hasOwner = true;
			GameLogic.Get().OutputGameLog("谈判成功，丽塔成为了盟友！\n");
		}
		else
		{
			GameLogic.Get().OutputGameLog("谈判失败！\n");
		}

		GameLogic.Get().AfterSkillTest(bSucceed, chaosToken);

		m_lstOtherLocations[3].OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
		ui.EndSelectCardToSpend();
	}
}
