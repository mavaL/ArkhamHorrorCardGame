using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MainGame : MonoBehaviour
{
	#region game UI
	public GameObject			m_actArea;
	public GameObject			m_agendaArea;
	public GameObject			m_gameArea;
	public List<GameObject>		m_playerCardArea;
	public List<GameObject>		m_threatArea;
	public Text					m_gameLog;
	public Text					m_confirmSkillTestText;
	public Button				m_InvestigateBtn;
	public Button				m_drawPlayerCardBtn;
	public Button				m_gainResourceBtn;
	public Button				m_enemyPhaseBtn;
	public Button				m_UpkeepPhaseBtn;
	public Button				m_MythosPhaseBtn;
	public Button				m_InvestigationPhaseBtn;
	public Button				m_advanceActBtn;
	public Button				m_confirmActResultBtn;
	public Button				m_confirmAgendaResultBtn;
	public Button				m_confirmEnterLocationBtn;
	public Button				m_confirmChoiceBtn;
	public Button				m_useLocationAbilityBtn;
	public Dropdown				m_movementDropdown;
	public Dropdown				m_choiceDropdown;
	public Dropdown				m_fightDropdown;
	#endregion

	// A single button functions many way
	public enum ConfirmButtonMode
	{
		None,
		GainCard,
		TextOnly,
		DrawPlayerCard,
		DrawEncounterCard,
		Investigate,
		SkillTest
	}
	[System.NonSerialized]
	public ConfirmButtonMode	m_choiceMode = ConfirmButtonMode.None;
	[System.NonSerialized]
	public List<UnityEvent> m_lstChoiceEvent = new List<UnityEvent>();
	// Used by LocationEvent
	[System.NonSerialized]
	public List<PlayerCard> m_lstCardChoice = new List<PlayerCard>(0);
	[System.NonSerialized]
	public GameObject		m_tempHighlightCard;

	string[]	m_roland_def_cards =
	{
		"Neutral/core_roland_dot38_special",
		"Neutral/core_cover_up",
		"Guardian/core_guardian_dot45_automatic",
		"Guardian/core_guardian_physical_training",
		"Guardian/core_guardian_beat_cop",
		"Guardian/core_guardian_first_aid",
		"Guardian/core_guardian_machete",
		"Guardian/core_guardian_dog",
		"Guardian/core_guardian_evidence",
		"Guardian/core_guardian_dodge",
		"Guardian/core_guardian_dynamite_blast",
		"Guardian/core_guardian_vicious_blow",
		"Seeker/core_seeker_magnifying_glass",
		"Seeker/core_seeker_old_book_of_lore",
		"Seeker/core_seeker_research_librarian",
		"Seeker/core_seeker_milan_christopher",
		"Seeker/core_seeker_hyperawreness",
		"Seeker/core_seeker_medical_texts",
		"Seeker/core_seeker_mind_over_matter",
		"Seeker/core_seeker_working_a_hunch",
		"Seeker/core_seeker_barricade",
		"Seeker/core_seeker_deduction",
		// TODO: 重复加载资源？
		"Neutral/core_knife",
		"Neutral/core_knife",
		"Neutral/core_flashlight",
		"Neutral/core_flashlight",
		"Neutral/core_emergency_cache",
		"Neutral/core_emergency_cache",
		"Neutral/core_guts",
		"Neutral/core_guts",
		"Neutral/core_manual_dexterity",
		"Neutral/core_manual_dexterity",
		"Neutral/core_paranoia",
	};

	private void Awake()
	{
		GameLogic.Get().m_logText = m_gameLog;
		GameLogic.Get().m_mainGameUI = this;
	}

	// Use this for initialization
	void Start ()
	{
		GameLogic.DockCard(Player.Get().m_investigatorCard.gameObject, GameObject.Find("InvestigatorCard"));

		_LoadPlayerCards(Player.Get().m_faction);
		_DrawFiveOpenHands();

		OnPlayerThreatAreaChnaged();
	}

	private void _DrawFiveOpenHands()
	{
		for(int i=0; i<5; ++i)
		{
			Player.Get().AddHandCard(GameLogic.Get().DrawPlayerCard());
		}
	}

	private void _LoadPlayerCards(Faction faction)
	{
		// Currently using default card set
		if(faction == Faction.Guardian)
		{
			for(int i=0; i<m_roland_def_cards.Length; ++i)
			{
				string path = "CardPrefabs/PlayerCards/" + m_roland_def_cards[i];
				GameObject card = Instantiate((GameObject)Resources.Load(path));
				card.SetActive(false);

				GameLogic.Get().m_lstPlayerCards.Add(card);
			}
		}
		else
		{
			Debug.LogError("Error!!Not implement...");
		}

		GameLogic.Shuffle(GameLogic.Get().m_lstPlayerCards);
	}

	public void	OnButtonDrawPlayerCard()
	{
		m_tempHighlightCard = GameLogic.Get().DrawPlayerCard();

		GameLogic.Get().ShowHighlightCardExclusive(m_tempHighlightCard.GetComponent<Card>(), false);

		m_confirmChoiceBtn.gameObject.SetActive(true);
		m_choiceMode = MainGame.ConfirmButtonMode.DrawPlayerCard;
	}

	public void OnButtonInvestigateCurrentLocation()
	{
		m_choiceMode = MainGame.ConfirmButtonMode.Investigate;
		BeginSelectCardToSpend();

		GameLogic.Get().OutputGameLog(string.Format("{0}调查了{1}\n", Player.Get().m_investigatorCard.m_cardName, Player.Get().m_currentLocation.m_cardName));
	}

	public void OnButtonGainOneResource()
	{
		Player.Get().m_resources += 1;
		GameLogic.Get().OutputGameLog(string.Format("{0}花费1行动获取了1资源\n", Player.Get().m_investigatorCard.m_cardName));
		Player.Get().ActionDone();
	}

	// Update is called once per frame
	void Update ()
	{
		// Display player hand cards
		var cards = Player.Get().GetHandCards();

		for(int i=0; i<cards.Count; ++i)
		{
			GameLogic.DockCard(cards[i].gameObject, m_playerCardArea[i]);
		}

		// Display player threat area
		var enemies = Player.Get().GetEnemyCards();

		for (int i = 0; i < enemies.Count; ++i)
		{
			GameLogic.DockCard(enemies[i].gameObject, m_threatArea[i]);
		}

		var scenario = GameLogic.Get().m_currentScenario;
		m_advanceActBtn.interactable = scenario.m_currentAct.m_currentToken + Player.Get().m_clues >= scenario.m_currentAct.m_tokenToAdvance;
		m_InvestigateBtn.interactable = Player.Get().m_currentLocation.m_clues > 0;

		GameLogic.Get().Update();
	}

	public void EnterEnemyPhase()
	{
		m_InvestigateBtn.gameObject.SetActive(false);
		m_drawPlayerCardBtn.gameObject.SetActive(false);
		m_gainResourceBtn.gameObject.SetActive(false);
		m_advanceActBtn.gameObject.SetActive(false);
		m_useLocationAbilityBtn.gameObject.SetActive(false);
		m_movementDropdown.gameObject.SetActive(false);
		m_fightDropdown.gameObject.SetActive(false);
		m_enemyPhaseBtn.gameObject.SetActive(true);

		GameLogic.Get().m_currentPhase = TurnPhase.EnemyPhase;
	}

	public void OnButtonEnterEnemyPhase()
	{
		GameLogic.Get().OutputGameLog("游戏进入敌人阶段\n");

		var enemies = GameLogic.m_lstUnengagedEnemyCards;

		// 1. Unengaged hunter enemies move
		if (enemies.Count > 0)
		{
			foreach(var enemy in enemies)
			{
				if(enemy.IsKeywordContain(Card.Keyword.Hunter))
				{
					enemy.HunterMoveToNearestInvestigator();
				}
			}
		}

		// 2. Resolve engaged enemy attacks
		Player.Get().ResolveEngagedEnemyAttack();

		m_enemyPhaseBtn.gameObject.SetActive(false);
		m_UpkeepPhaseBtn.gameObject.SetActive(true);
	}

	public void OnButtonEnterUpkeepPhase()
	{
		GameLogic.Get().OutputGameLog("游戏进入维持阶段\n");
		GameLogic.Get().m_currentPhase = TurnPhase.UpkeepPhase;

		// 1. Reset actions
		Player.Get().ResetAction();

		// 2. Ready exhausted cards
		foreach(var exhausted in GameLogic.m_lstExhaustedCards)
		{
			exhausted.m_exhausted = false;
		}
		GameLogic.m_lstExhaustedCards.Clear();

		// 3. Each investigator draws 1 card and gains 1 resource
		var card = GameLogic.Get().DrawPlayerCard();
		Player.Get().AddHandCard(card);
		Player.Get().m_resources += 1;

		GameLogic.Get().OutputGameLog(string.Format("{0}在维持阶段获得了1资源，1手牌<{1}>\n", Player.Get().m_investigatorCard.m_cardName, card.GetComponent<Card>().m_cardName));

		// 4. Each investigator checks hand size

		m_UpkeepPhaseBtn.gameObject.SetActive(false);
		m_MythosPhaseBtn.gameObject.SetActive(true);
	}

	public void OnButtonEnterMythosPhase()
	{
		GameLogic.Get().OutputGameLog("游戏进入神话阶段\n");
		GameLogic.Get().m_currentPhase = TurnPhase.MythosPhase;

		// Add 1 doom token
		var agenda = GameLogic.Get().m_currentScenario.m_currentAgenda;
		if (agenda.AddDoom())
		{
			GameLogic.Get().OutputGameLog("恶兆已集满，触发事件！\n");

			GameLogic.Get().ShowHighlightCardExclusive(agenda, true);

			m_confirmAgendaResultBtn.gameObject.SetActive(true);
		}
		else
		{
			m_InvestigationPhaseBtn.gameObject.SetActive(true);
		}

		// Draw and reveal a encounter card
		m_tempHighlightCard = GameLogic.Get().DrawEncounterCard();
		GameLogic.Get().ShowHighlightCardExclusive(m_tempHighlightCard.GetComponent<Card>(), false);

		m_confirmChoiceBtn.gameObject.SetActive(true);
		m_choiceMode = MainGame.ConfirmButtonMode.DrawEncounterCard;
		m_MythosPhaseBtn.gameObject.SetActive(false);
	}

	public void OnButtonEnterInvestigationPhase()
	{
		GameLogic.Get().OutputGameLog("游戏进入调查阶段\n");
		GameLogic.Get().m_currentPhase = TurnPhase.InvestigationPhase;

		m_InvestigationPhaseBtn.gameObject.SetActive(false);
		m_InvestigateBtn.interactable = Player.Get().m_currentLocation.m_clues > 0;
		m_enemyPhaseBtn.gameObject.SetActive(false);
		m_advanceActBtn.gameObject.SetActive(true);

		m_InvestigateBtn.gameObject.SetActive(true);
		m_drawPlayerCardBtn.gameObject.SetActive(true);
		m_gainResourceBtn.gameObject.SetActive(true);
		m_advanceActBtn.gameObject.SetActive(true);
		m_movementDropdown.gameObject.SetActive(true);
		m_fightDropdown.gameObject.SetActive(true);
		m_useLocationAbilityBtn.gameObject.SetActive(Player.Get().m_currentLocation.m_locationAbilityCallback.GetPersistentEventCount() > 0);
	}

	public void OnButtonAdvanceAct()
	{
		var scenario = GameLogic.Get().m_currentScenario;
		Player.Get().m_clues -= scenario.m_currentAct.m_tokenToAdvance - scenario.m_currentAct.m_currentToken;

		UnityEngine.Assertions.Assert.IsTrue(Player.Get().m_clues >= 0, "Assert failed in OnButtonAdvanceAct()!!");

		// Show player the result of the current act.
		GameLogic.Get().ShowHighlightCardExclusive(scenario.m_currentAct, true);

		m_confirmActResultBtn.gameObject.SetActive(true);
	}

	// 0 means ActCard, 1 means LocationCard, 2 means AgendaCard
	public void OnButtonConfirmCardHighlight(int type)
	{
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = true;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = true;

		if (type == 0)
		{
			m_confirmActResultBtn.gameObject.SetActive(false);

			var scenario = GameLogic.Get().m_currentScenario;
			scenario.m_currentAct.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));

			// Show next act or reach the end
			scenario.AdvanceAct();
		}
		else if(type == 1)
		{
			m_confirmEnterLocationBtn.gameObject.SetActive(false);

			Player.Get().m_currentLocation.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
			Player.Get().m_currentLocation.EnterLocation();
		}
		else if(type == 2)
		{
			m_confirmAgendaResultBtn.gameObject.SetActive(false);

			var scenario = GameLogic.Get().m_currentScenario;
			scenario.m_currentAgenda.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));

			scenario.AdvanceAgenda();
		}
	}

	public void OnMovementDestinationChanged(Dropdown d)
	{
		if(d.value == 0)
		{
			// No movement
			return;
		}

		string locName = d.options[d.value].text;
		var locList = GameLogic.Get().m_currentScenario.m_lstOtherLocations;

		for(int i=0; i<locList.Count; ++i)
		{
			if(locList[i].m_cardName == locName)
			{
				GameLogic.Get().PlayerEnterLocation(locList[i].gameObject);
				break;
			}
		}
	}

	public void OnButtonUseLocationAbility()
	{
		Player.Get().m_currentLocation.m_locationAbilityCallback.Invoke(Player.Get().m_currentLocation);
	}

	public void OnChoiceChanged(Dropdown d)
	{
		if(m_choiceMode == ConfirmButtonMode.GainCard)
		{
			if (GameLogic.Get().m_highlightCard != null)
			{
				GameLogic.Get().m_highlightCard.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
			}
			GameLogic.Get().ShowHighlightCardExclusive(m_lstCardChoice[d.value], false);
		}
		else if (m_choiceMode == ConfirmButtonMode.TextOnly)
		{
			GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = false;
			GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = false;
		}
	}

	public void OnButtonConfirmChoice()
	{
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = true;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = true;

		m_confirmChoiceBtn.gameObject.SetActive(false);

		if (m_choiceMode == ConfirmButtonMode.GainCard)
		{
			PlayerCard card = m_lstCardChoice[m_choiceDropdown.value];
			Player.Get().AddHandCard(card.gameObject);

			GameLogic.Get().m_lstPlayerCards.Remove(card.gameObject);
			GameLogic.Shuffle(GameLogic.Get().m_lstPlayerCards);

			m_choiceDropdown.gameObject.SetActive(false);
			card.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
			GameLogic.Get().OutputGameLog(string.Format("{0}获取了手牌：{1}\n", Player.Get().m_investigatorCard.m_cardName, card.m_cardName));
		}
		else if (m_choiceMode == ConfirmButtonMode.TextOnly)
		{
			m_choiceDropdown.gameObject.SetActive(false);
			m_lstChoiceEvent[m_choiceDropdown.value].Invoke();
		}
		else if(m_choiceMode == ConfirmButtonMode.DrawPlayerCard)
		{
			UnityEngine.Assertions.Assert.IsNotNull(m_tempHighlightCard, "Assert failed in OnButtonConfirmChoice()-DrawPlayerCard!!");

			m_tempHighlightCard.GetComponent<Card>().OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
			Player.Get().AddHandCard(m_tempHighlightCard);

			GameLogic.Get().OutputGameLog(string.Format("{0}花费1行动获取了<{1}>\n", Player.Get().m_investigatorCard.m_cardName, m_tempHighlightCard.GetComponent<PlayerCard>().m_cardName));
			Player.Get().ActionDone();
			m_tempHighlightCard = null;
		}
		else if(m_choiceMode == ConfirmButtonMode.DrawEncounterCard)
		{
			UnityEngine.Assertions.Assert.IsNotNull(m_tempHighlightCard, "Assert failed in OnButtonConfirmChoice()-DrawEncounterCard!!");
			Card card = m_tempHighlightCard.GetComponent<Card>();
			GameLogic.Get().OutputGameLog(string.Format("{0}摸到了遭遇卡<{1}>\n", Player.Get().m_investigatorCard.m_cardName, card.m_cardName));

			if(card is EnemyCard)
			{
				EnemyCard enemy = card as EnemyCard;
				Player.Get().AddEngagedEnemy(enemy);

				card.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
				m_tempHighlightCard = null;
			}
			else if(card is TreacheryCard)
			{
				GameLogic.Get().m_mainGameUI.m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;
				GameLogic.Get().m_mainGameUI.BeginSelectCardToSpend();		
			}
		}
		else if(m_choiceMode == ConfirmButtonMode.Investigate)
		{
			GameLogic.Get().InvestigateCurrentLocation();
			EndSelectCardToSpend();
		}
		else if(m_choiceMode == ConfirmButtonMode.SkillTest)
		{
			Card card = m_tempHighlightCard.GetComponent<Card>();
			card.OnSkillTest();

			card.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
			m_tempHighlightCard = null;
			EndSelectCardToSpend();
		}
	}

	public void BeginSelectCardToSpend()
	{
		m_confirmChoiceBtn.gameObject.SetActive(true);	
		m_confirmSkillTestText.gameObject.SetActive(true);
		m_gameArea.SetActive(false);
		GameLogic.Get().m_cardClickMode = Card.CardClickMode.MultiSelect;

		// ...................Seems like Unity's BUG.......................
		ScrollRect dropDownList = m_fightDropdown.GetComponentInChildren<ScrollRect>();
		if (dropDownList != null)
		{
			Destroy(dropDownList.gameObject);
		}
	}

	public void EndSelectCardToSpend()
	{
		Card.m_lstSelectCards.ForEach(card =>
		{
			// Restore position
			RectTransform rt = (RectTransform)card.gameObject.GetComponent<RectTransform>().parent;
			rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y - 30);

			card.Discard();
		});

		Card.m_lstSelectCards.Clear();
		m_gameArea.SetActive(true);
		GameLogic.Get().m_cardClickMode = Card.CardClickMode.Flip;
		m_confirmSkillTestText.gameObject.SetActive(false);
	}

	public void OnFightTargetChanged(Dropdown d)
	{
		// Option 0 is reserved
		if(d.value == 0)
		{
			return;
		}

		GameLogic.Get().PlayerFightEnemy(Player.Get().GetEnemyCards()[d.value-1]);
	}

	public void OnPlayerThreatAreaChnaged()
	{
		m_fightDropdown.ClearOptions();

		List<string> cardNames = new List<string>();
		cardNames.Add("请选择攻击目标...");
		var enemies = Player.Get().GetEnemyCards();

		enemies.ForEach(enemy => { cardNames.Add(enemy.m_cardName); });
		m_fightDropdown.AddOptions(cardNames);
		m_fightDropdown.RefreshShownValue();
	}
}
