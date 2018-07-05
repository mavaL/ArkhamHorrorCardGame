using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ChoiceEvent : UnityEvent<object>
{
	public object	m_eventParam { get; }

	public ChoiceEvent()
	{
		m_eventParam = null;
	}

	public ChoiceEvent(object eventParam)
	{
		m_eventParam = eventParam;
	}
}

public class MainGame : MonoBehaviour
{
	#region game UI
	public GameObject			m_actArea;
	public GameObject			m_agendaArea;
	public GameObject			m_gameArea;
	public Text					m_gameLog;
	public Text					m_confirmChoiceText;
	public Text					m_statsInfoText;
	public Scrollbar			m_logScrollBar;
	public Scrollbar			m_statsScrollBar;
	public GameObject			m_endingPanel;

	public ActionDropdownGUI	m_actionGUI;
	public Dictionary<PlayerAction, bool> m_isActionEnable { get; set; } = new Dictionary<PlayerAction, bool>();

	public Button m_enemyPhaseBtn;
	public Button m_UpkeepPhaseBtn;
	public Button m_MythosPhaseBtn;
	public Button m_InvestigationPhaseBtn;
	public Button m_advanceActBtn;
	public Button m_confirmActResultBtn;
	public Button m_confirmAgendaResultBtn;
	public Button m_confirmEnterLocationBtn;
	public Button m_confirmToMainMenuBtn;
	public Button m_confirmChoiceBtn;
	public Button m_useLocationAbilityBtn;

	public Dropdown m_targetDropdown;
	public Dropdown m_choiceDropdown;
	public Dropdown m_actionDropdown;
	
	public CardListView	m_handCardListView;
	public CardListView	m_threatListView;
	public CardListView	m_assetListView;
	#endregion

	// A single button functions many way
	public enum ConfirmButtonMode
	{
		None,
		GainCard,
		TextOnly,
		RevealCard,
		DrawEncounterCard,
		SkillTest,
		DiscardExcessHandCards,
		Custom
	}

	public ConfirmButtonMode m_choiceMode { get; set; } = ConfirmButtonMode.None;
	public List<ChoiceEvent>	m_lstChoiceEvent { get; set; } = new List<ChoiceEvent>();
	// Used by LocationEvent
	public List<PlayerCard> m_lstCardChoice { get; set; } = new List<PlayerCard>(0);
	public GameObject		m_tempHighlightCard { get; set; }
	public bool				m_bConfirmModeEnd { get; set; }
	public UnityEvent		m_investigatePhaseBeginEvent { get; set; } = new UnityEvent();
	public UnityEvent		m_enemyPhaseBeginEvent { get; set; } = new UnityEvent();
	public UnityEvent		m_upkeepPhaseBeginEvent { get; set; } = new UnityEvent();
	public UnityEvent		m_mythosPhaseBeginEvent { get; set; } = new UnityEvent();
	public UnityEvent		m_roundEndEvent { get; set; } = new UnityEvent();
	private UnityAction<int> m_onPlayReactiveEvent;
	private UnityAction<int> m_onUseReactiveAsset;

	string[] m_roland_def_cards =
	{
		"Neutral/core_guts",
		"Neutral/core_guts",
		"Neutral/core_flashlight",
		"Neutral/core_emergency_cache",
		"Seeker/core_seeker_deduction",
		"Seeker/core_seeker_working_a_hunch",
		"Seeker/core_seeker_research_librarian",
		"Seeker/core_seeker_barricade",
		"Seeker/core_seeker_old_book_of_lore",
		"Guardian/core_guardian_dog",
		"Guardian/core_guardian_evidence",
		"Seeker/core_seeker_mind_over_matter",
		"Seeker/core_seeker_milan_christopher",
		"Seeker/core_seeker_medical_texts",
		"Seeker/core_seeker_magnifying_glass",
		"Seeker/core_seeker_hyperawreness",
		"Guardian/core_guardian_vicious_blow",
		"Guardian/core_guardian_physical_training",
		"Guardian/core_guardian_machete",
		"Guardian/core_guardian_first_aid",
		"Guardian/core_guardian_dynamite_blast",
		"Guardian/core_guardian_beat_cop",
		"Guardian/core_guardian_dot45_automatic",
		"Guardian/core_guardian_dodge",
		"Neutral/core_roland_dot38_special",
		// TODO: 重复加载资源？
		"Neutral/core_knife",
		"Neutral/core_knife",
		"Neutral/core_flashlight",
		"Neutral/core_emergency_cache",
		"Neutral/core_manual_dexterity",
		"Neutral/core_manual_dexterity",
		"Neutral/core_paranoia",
		"Neutral/core_cover_up",
	};

	private void Awake()
	{
		GameLogic.Get().m_mainGameUI = this;

		m_isActionEnable.Add(PlayerAction.Move, true);
		m_isActionEnable.Add(PlayerAction.Investigate, true);
		m_isActionEnable.Add(PlayerAction.Evade, true);
		m_isActionEnable.Add(PlayerAction.Fight, true);
		m_isActionEnable.Add(PlayerAction.DrawOneCard, true);
		m_isActionEnable.Add(PlayerAction.GainOneResource, true);
		m_isActionEnable.Add(PlayerAction.PlayCard, true);
		m_isActionEnable.Add(PlayerAction.NonStandardAction1, true);
		m_isActionEnable.Add(PlayerAction.NonStandardAction2, true);
		m_isActionEnable.Add(PlayerAction.NonStandardAction3, true);
		m_isActionEnable.Add(PlayerAction.NonStandardAction4, true);
		m_isActionEnable.Add(PlayerAction.NonStandardAction5, true);
		m_isActionEnable.Add(PlayerAction.NonStandardAction6, true);
		m_isActionEnable.Add(PlayerAction.NonStandardAction7, true);
		m_isActionEnable.Add(PlayerAction.NonStandardAction8, true);
		m_isActionEnable.Add(PlayerAction.NonStandardAction9, true);
		m_isActionEnable.Add(PlayerAction.NonStandardAction10, true);

		m_onPlayReactiveEvent = new UnityAction<int>(OnPlayReactiveEvent);
		m_onUseReactiveAsset = new UnityAction<int>(OnUseReactiveAsset);
	}

	// Use this for initialization
	void Start()
	{
		m_handCardListView.Init();
		m_threatListView.Init();
		m_assetListView.Init();

		GameLogic.DockCard(Player.Get().m_investigatorCard.gameObject, GameObject.Find("InvestigatorCard"));

		_LoadPlayerCards(Player.Get().m_faction);
		_DrawFiveOpenHands();
	}

	private void _DrawFiveOpenHands()
	{
		for (int i = 0; i < 5; ++i)
		{
			Player.Get().AddHandCard(GameLogic.Get().DrawPlayerCard());
		}
	}

	private void _LoadPlayerCards(Faction faction)
	{
		// Currently using default card set
		if (faction == Faction.Guardian)
		{
			for (int i = 0; i < m_roland_def_cards.Length; ++i)
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

		//GameLogic.Shuffle(GameLogic.Get().m_lstPlayerCards);
	}

	public void DrawPlayerCard()
	{
		m_tempHighlightCard = GameLogic.Get().DrawPlayerCard();

		GameLogic.Get().ShowHighlightCardExclusive(m_tempHighlightCard.GetComponent<Card>(), false);

		m_confirmChoiceBtn.gameObject.SetActive(true);
		m_choiceMode = MainGame.ConfirmButtonMode.RevealCard;
	}

	public void OnActionDropdownChange(Dropdown d)
	{
		if (d.value == 0)
		{
			return;
		}

		m_actionDropdown.gameObject.SetActive(false);

		// Only process standard actions
		if (d.value >= (int)PlayerAction.NonStandardAction1)
		{
			return;
		}

		Player.Get().m_currentAction.Push((PlayerAction)d.value);

		switch (Player.Get().GetCurrentAction())
		{
			case PlayerAction.Investigate:
				InvestigateCurrentLocation();
				break;
			case PlayerAction.Move:
			case PlayerAction.Evade:
			case PlayerAction.Fight:
			case PlayerAction.PlayCard:
				UpdateTargetDropdown();
				m_targetDropdown.gameObject.SetActive(true);
				break;
			case PlayerAction.DrawOneCard:
				DrawPlayerCard();
				break;
			case PlayerAction.GainOneResource:
				GainOneResource();
				break;
			case PlayerAction.Skip:
				Player.Get().m_currentAction.Pop();
				EnterEnemyPhase();
				break;
		}
	}

	public void InvestigateCurrentLocation()
	{
		GameLogic.Get().OutputGameLog(string.Format("{0}调查了{1}\n", Player.Get().m_investigatorCard.m_cardName, Player.Get().m_currentLocation.m_cardName));

		m_tempHighlightCard = Player.Get().m_currentLocation.gameObject;
		m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;

		BeginSelectCardToSpend(SkillType.Intellect);
	}

	public void GainOneResource()
	{
		Player.Get().m_resources += 1;
		GameLogic.Get().OutputGameLog(string.Format("{0}花费1行动获取了1资源\n", Player.Get().m_investigatorCard.m_cardName));
		Player.Get().ActionDone(PlayerAction.GainOneResource);
	}

	// Update is called once per frame
	void Update()
	{
		m_advanceActBtn.interactable = GameLogic.Get().CanAdvanceAct();

		if(GameLogic.Get().m_currentPhase == TurnPhase.ScenarioEnd)
		{
			m_actionDropdown.gameObject.SetActive(false);
		}

		GameLogic.Get().Update();
	}

	public void EnterEnemyPhase()
	{
		StartCoroutine(_EnterEnemyPhase());
	}

	private IEnumerator _EnterEnemyPhase()
	{
		yield return new WaitUntil(() => GameLogic.Get().m_currentTiming == EventTiming.None);

		m_actionDropdown.gameObject.SetActive(false);
		m_useLocationAbilityBtn.gameObject.SetActive(false);
		m_enemyPhaseBtn.gameObject.SetActive(true);

		GameLogic.Get().m_currentPhase = TurnPhase.EnemyPhase;
	}

	public void OnButtonEnterEnemyPhase()
	{
		GameLogic.Get().OutputGameLog("游戏进入敌人阶段\n");
		m_advanceActBtn.gameObject.SetActive(false);
		m_enemyPhaseBtn.gameObject.SetActive(false);

		m_enemyPhaseBeginEvent.Invoke();

		var enemies = GameLogic.m_lstUnengagedEnemyCards;

		// 1. Unengaged hunter enemies move
		for(int i=0; i< enemies.Count; ++i)
		{
			var enemy = enemies[i];
		
			if (enemy.IsKeywordContain(Card.Keyword.Hunter) && enemy.HunterMoveToNearestInvestigator())
			{
				if (enemy.m_currentLocation.m_cardName == Player.Get().m_currentLocation.m_cardName)
				{
					Player.Get().AddEngagedEnemy(enemy);
					--i;
				}
			}
		}

		// 2. Resolve engaged enemy attacks
		StartCoroutine(OnAllEnemyAttack());
	}

	IEnumerator OnAllEnemyAttack()
	{
		var enemies = Player.Get().GetEnemyCards();
		// Workaround for discarding in the middle of looping
		var tmpList = new List<EnemyCard>(enemies);

		for(int i=0; i< tmpList.Count; ++i)
		{
			if(OnEventTiming(EventTiming.EnemyAttack))
			{
				GameLogic.Get().ShowHighlightCardExclusive(tmpList[i], false, false);
				yield return new WaitUntil(() => GameLogic.Get().m_currentTiming == EventTiming.None);
			}

			StartCoroutine(ResolveEngagedEnemyAttack(tmpList[i]));

			yield return new WaitUntil(() => Player.Get().m_currentAction.Count == 0);
		}

		m_UpkeepPhaseBtn.gameObject.SetActive(true);
	}

	private IEnumerator ResolveEngagedEnemyAttack(EnemyCard enemy)
	{
		GameLogic.Get().m_enemyAttackEvent.Invoke();

		if (!enemy.m_exhausted)
		{
			GameLogic.Get().OutputGameLog(string.Format("{0}被<{1}>攻击，受到：{2}点伤害，{3}点恐怖！\n", Player.Get().m_investigatorCard.m_cardName, enemy.m_cardName, enemy.m_damage, enemy.m_horror));

			if(enemy.m_damage > 0)
			{
				Player.Get().DecreaseHealth(enemy, enemy.m_damage);
			}

			yield return new WaitUntil(() => Player.Get().m_currentAction.Count == 0);

			if (enemy.m_horror > 0)
			{
				Player.Get().DecreaseSanity(enemy.m_horror);
			}

			enemy.OnExhausted();
		}
	}

	public void OnPlayReactiveEvent(int index)
	{
		if(index < 1)
		{
			return;
		}

		m_targetDropdown.onValueChanged.RemoveListener(m_onPlayReactiveEvent);

		var highlightCard = m_tempHighlightCard;
		m_tempHighlightCard.GetComponent<Card>().OnPointerExit(null);
		m_tempHighlightCard = null;

		var cards = Player.Get().GetHandCards();

		for (int i = 0; i < cards.Count; ++i)
		{
			if (cards[i].m_cardName == m_targetDropdown.options[index].text)
			{
				cards[i].GetComponent<PlayerCardLogic>().OnPlayReactiveEvent(highlightCard.GetComponent<Card>());
				Player.Get().m_resources -= cards[i].m_cost;

				UnityEngine.Assertions.Assert.IsTrue(Player.Get().m_resources >= 0, "Assert failed in MainGame.OnPlayReactiveEvent()!!!");

				cards[i].Discard();
				break;
			}
		}		

		GameLogic.Get().m_currentTiming = EventTiming.None;
		Player.Get().m_currentAction.Pop();

		m_targetDropdown.gameObject.SetActive(false);
		m_actionDropdown.gameObject.SetActive(GameLogic.Get().m_currentPhase == TurnPhase.InvestigationPhase);
	}

	public bool OnEventTiming(EventTiming timing)
	{
		if (Player.Get().CanPlayEvent(timing))
		{
			GameLogic.Get().m_currentTiming = timing;
			Player.Get().m_currentAction.Push(PlayerAction.ReactiveEvent);

			m_actionDropdown.gameObject.SetActive(false);
			m_targetDropdown.gameObject.SetActive(true);
			UpdateTargetDropdown();
			m_targetDropdown.onValueChanged.AddListener(m_onPlayReactiveEvent);

			return true;
		}
		return false;
	}

	public bool OnAssetTiming(EventTiming timing)
	{
		if (Player.Get().CanTriggerAsset(timing))
		{
			GameLogic.Get().m_currentTiming = timing;
			Player.Get().m_currentAction.Push(PlayerAction.ReactiveAsset);

			m_actionDropdown.gameObject.SetActive(false);
			m_targetDropdown.gameObject.SetActive(true);
			UpdateTargetDropdown();
			m_targetDropdown.onValueChanged.AddListener(m_onUseReactiveAsset);

			return true;
		}
		return false;
	}

	public void OnUseReactiveAsset(int index)
	{
		if (index < 1)
		{
			return;
		}

		m_targetDropdown.onValueChanged.RemoveListener(m_onUseReactiveAsset);

		if(index == 1)
		{
			// Call below in PlayerCardLogic after using reactive asset
			GameLogic.Get().m_currentTiming = EventTiming.None;
			Player.Get().m_currentAction.Pop();
		}
		else
		{
			var cards = Player.Get().GetAssetAreaCards();

			for (int i = 0; i < cards.Count; ++i)
			{
				if (cards[i].m_cardName == m_targetDropdown.options[index].text)
				{
					cards[i].GetComponent<PlayerCardLogic>().OnUseReactiveAsset();
					break;
				}
			}
		}

		m_targetDropdown.gameObject.SetActive(false);
	}

	public void OnButtonEnterUpkeepPhase()
	{
		GameLogic.Get().OutputGameLog("游戏进入维持阶段\n");
		GameLogic.Get().m_currentPhase = TurnPhase.UpkeepPhase;

		m_upkeepPhaseBeginEvent.Invoke();

		// 1. Reset actions
		Player.Get().ResetAction();

		// 2. Ready exhausted cards
		foreach (var exhausted in GameLogic.m_lstExhaustedCards)
		{
			exhausted.OnRecoverFromExhaust();
		}
		GameLogic.m_lstExhaustedCards.Clear();

		// 3. Each investigator draws 1 card and gains 1 resource
		var card = GameLogic.Get().DrawPlayerCard();
		Player.Get().AddHandCard(card);
		Player.Get().m_resources += 1;

		GameLogic.Get().OutputGameLog(string.Format("{0}在维持阶段获得了1资源，1手牌<{1}>\n", Player.Get().m_investigatorCard.m_cardName, card.GetComponent<Card>().m_cardName));

		// 4. Each investigator checks hand size
		StartCoroutine(_CheckHandCardLimit());

		m_UpkeepPhaseBtn.gameObject.SetActive(false);
		m_MythosPhaseBtn.gameObject.SetActive(true);

		m_roundEndEvent.Invoke();
	}

	private IEnumerator _CheckHandCardLimit()
	{
		while(Player.Get().GetHandCards().Count > 8)
		{
			m_confirmChoiceBtn.gameObject.SetActive(true);
			m_choiceMode = MainGame.ConfirmButtonMode.DiscardExcessHandCards;

			BeginSelectCardToSpend(SkillType.None);

			yield return new WaitUntil(() => m_bConfirmModeEnd == true);
		}
	}

	public void OnButtonEnterMythosPhase()
	{
		StartCoroutine(OnButtonEnterMythosPhase_Coroutine());
	}

	private IEnumerator OnButtonEnterMythosPhase_Coroutine()
	{
		GameLogic.Get().OutputGameLog("游戏进入神话阶段\n");
		GameLogic.Get().m_currentPhase = TurnPhase.MythosPhase;

		m_mythosPhaseBeginEvent.Invoke();

		// Add 1 doom token
		var agenda = GameLogic.Get().m_currentScenario.m_currentAgenda;
		if (agenda.AddDoom())
		{
			GameLogic.Get().OutputGameLog("恶兆已集满，触发事件！\n");
			GameLogic.Get().ShowHighlightCardExclusive(agenda, true);
			m_confirmAgendaResultBtn.gameObject.SetActive(true);

			m_bConfirmModeEnd = false;
			yield return new WaitUntil(() => m_bConfirmModeEnd == true);
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

		m_investigatePhaseBeginEvent.Invoke();

		m_InvestigationPhaseBtn.gameObject.SetActive(false);
		m_enemyPhaseBtn.gameObject.SetActive(false);
		m_advanceActBtn.gameObject.SetActive(true);
		m_advanceActBtn.gameObject.SetActive(true);
		m_useLocationAbilityBtn.gameObject.SetActive(Player.Get().m_currentLocation.m_locationAbilityCallback.GetPersistentEventCount() > 0);

		ResetActionDropdown();
	}

	public void OnButtonAdvanceAct(bool bUseClues = true)
	{
		var scenario = GameLogic.Get().m_currentScenario;

		if(bUseClues)
		{
			Player.Get().m_clues -= scenario.m_currentAct.m_tokenToAdvance - scenario.m_currentAct.m_currentToken;
			UnityEngine.Assertions.Assert.IsTrue(Player.Get().m_clues >= 0, "Assert failed in OnButtonAdvanceAct()!!");
		}

		// Show player the result of the current act.
		GameLogic.Get().ShowHighlightCardExclusive(scenario.m_currentAct, true);

		m_confirmActResultBtn.gameObject.SetActive(true);
	}

	// 0 means ActCard, 1 means LocationCard, 2 means AgendaCard, 3 means ReturnToMainMenu
	public void OnButtonConfirmCardHighlight(int type)
	{
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = true;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = true;

		if (type == 0)
		{
			m_confirmActResultBtn.gameObject.SetActive(false);

			var scenario = GameLogic.Get().m_currentScenario;
			scenario.m_currentAct.OnPointerExit(null);

			// Show next act or reach the end
			scenario.AdvanceAct();
		}
		else if(type == 1)
		{
			m_confirmEnterLocationBtn.gameObject.SetActive(false);

			Player.Get().m_currentLocation.OnPointerExit(null);
			Player.Get().m_currentLocation.EnterLocation();
			m_bConfirmModeEnd = true;
		}
		else if(type == 2)
		{
			m_confirmAgendaResultBtn.gameObject.SetActive(false);

			var scenario = GameLogic.Get().m_currentScenario;
			scenario.m_currentAgenda.OnPointerExit(null);

			scenario.AdvanceAgenda();
		}
		else if(type == 3)
		{
			m_confirmToMainMenuBtn.gameObject.SetActive(false);
			UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
		}
	}

	public void OnButtonUseLocationAbility()
	{
		Player.Get().m_currentLocation.m_locationAbilityCallback.Invoke(null);
	}

	public void OnChoiceChanged(Dropdown d)
	{
		if(m_choiceMode == ConfirmButtonMode.GainCard)
		{
			if (m_tempHighlightCard != null)
			{
				m_tempHighlightCard.GetComponent<Card>().OnPointerExit(null);
			}
			GameLogic.Get().ShowHighlightCardExclusive(m_lstCardChoice[d.value], false);
		}
		else if (m_choiceMode == ConfirmButtonMode.TextOnly)
		{
			//GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = false;
			//GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = false;
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
			card.OnPointerExit(null);
			GameLogic.Get().OutputGameLog(string.Format("{0}获取了手牌：{1}\n", Player.Get().m_investigatorCard.m_cardName, card.m_cardName));
		}
		else if (m_choiceMode == ConfirmButtonMode.TextOnly)
		{
			m_choiceDropdown.gameObject.SetActive(false);
			m_lstChoiceEvent[m_choiceDropdown.value].Invoke(m_lstChoiceEvent[m_choiceDropdown.value].m_eventParam);
			m_choiceDropdown.value = 0;
		}
		else if(m_choiceMode == ConfirmButtonMode.RevealCard)
		{
			UnityEngine.Assertions.Assert.IsNotNull(m_tempHighlightCard, "Assert failed in OnButtonConfirmChoice()-DrawPlayerCard!!");
			Card card = m_tempHighlightCard.GetComponent<Card>();

			if (card is PlayerCard)
			{			
				PlayerCard pc = card as PlayerCard;
				if(pc.m_isPlayerDeck)
				{
					Player.Get().AddHandCard(m_tempHighlightCard);

					GameLogic.Get().OutputGameLog(string.Format("{0}花费1行动获取了<{1}>\n", Player.Get().m_investigatorCard.m_cardName, card.m_cardName));
					Player.Get().ActionDone(PlayerAction.DrawOneCard);
				}
			}
			else if(card is LocationCard)
			{
				GameLogic.Get().OutputGameLog(string.Format("地点<{0}>被揭示\n", card.m_cardName));
			}

			card.OnPointerExit(null);
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
				if(enemy.m_spawnLocation != null)
				{
					// Prefab has no instance transform info, so we need to find it
					var foundObjects = FindObjectsOfType<LocationCard>();
					foreach(var location in foundObjects)
					{
						if(location.m_cardName == enemy.m_spawnLocation.m_cardName)
						{
							GameLogic.DockCard(card.gameObject, location.gameObject, 300, true, true);
							card.OnSpawnAtLocation(location);
							break;
						}
					}
				}
				else
				{
					Player.Get().AddEngagedEnemy(enemy);
				}

				card.OnPointerExit(null);
				m_tempHighlightCard = null;
			}
			else if(card is TreacheryCard)
			{
				card.OnPointerExit(null);
				m_tempHighlightCard = null;

				TreacheryLogic treachery = card.GetComponent<TreacheryLogic>();
				treachery.OnReveal();
				
			}
		}
		else if(m_choiceMode == ConfirmButtonMode.SkillTest)
		{
			StartCoroutine(OnConfirmSkillTest());
		}
		else if(m_choiceMode == ConfirmButtonMode.DiscardExcessHandCards)
		{
			EndSelectCardToSpend();
		}

		m_bConfirmModeEnd = true;
	}

	private IEnumerator OnConfirmSkillTest()
	{
		Card card = m_tempHighlightCard.GetComponent<Card>();
		card.OnSkillTest();
		EndSelectCardToSpend();

		yield return new WaitUntil(() => GameLogic.Get().m_currentTiming == EventTiming.None);

		card.OnPointerExit(null);
		m_tempHighlightCard = null;
	}

	public void OnPlayerFightEnemy(EnemyCard enemy)
	{
		GameLogic.Get().OutputGameLog(string.Format("{0}对{1}发动了攻击\n", Player.Get().m_investigatorCard.m_cardName, enemy.m_cardName));

		m_confirmChoiceBtn.gameObject.SetActive(true);
		m_tempHighlightCard = enemy.gameObject;
		m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;
		BeginSelectCardToSpend(SkillType.Combat);
	}

	public void OnPlayerEvadeEnemy(EnemyCard enemy)
	{
		GameLogic.Get().OutputGameLog(string.Format("{0}试图闪避{1}\n", Player.Get().m_investigatorCard.m_cardName, enemy.m_cardName));

		m_confirmChoiceBtn.gameObject.SetActive(true);
		m_tempHighlightCard = enemy.gameObject;
		m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;
		BeginSelectCardToSpend(SkillType.Agility);
	}

	public void ResetActionDropdown()
	{
		StartCoroutine(_ResetActionDropdown());
	}

	private IEnumerator _ResetActionDropdown()
	{
		yield return new WaitUntil(() => GameLogic.Get().m_currentTiming == EventTiming.None);

		m_actionDropdown.gameObject.SetActive(true);
		m_actionDropdown.value = 0;

		if(Player.Get().m_currentAction.Count > 0)
		{
			Player.Get().m_currentAction.Pop();
		}

		// ...................Seems like Unity's BUG.......................
		ScrollRect dropDownList = m_actionDropdown.GetComponentInChildren<ScrollRect>();
		if (dropDownList != null)
		{
			Destroy(dropDownList.gameObject);
		}
	}

	public void BeginSelectCardToSpend(SkillType type)
	{
		StartCoroutine(_BeginSelectCardToSpend(type));
	}

	private IEnumerator _BeginSelectCardToSpend(SkillType type)
	{
		if (m_choiceMode == ConfirmButtonMode.SkillTest && OnAssetTiming(EventTiming.BeforeSkillTest))
		{
			yield return new WaitUntil(() => GameLogic.Get().m_currentTiming == EventTiming.None);
		}

		var saveChoiceMode = m_choiceMode;
		GameLogic.Get().m_beforeSkillTest.Invoke(type);
		yield return new WaitUntil(() => GameLogic.Get().m_currentTiming == EventTiming.None);
		m_choiceMode = saveChoiceMode;

		m_gameArea.SetActive(false);
		m_confirmChoiceBtn.gameObject.SetActive(true);
		m_confirmChoiceText.gameObject.SetActive(true);
		GameLogic.Get().m_cardClickMode = Card.CardClickMode.MultiSelect;

		if (m_choiceMode == ConfirmButtonMode.SkillTest)
		{
			m_confirmChoiceText.text = "请选择参与检定的手牌：";
		}
		else if (m_choiceMode == ConfirmButtonMode.DiscardExcessHandCards)
		{
			m_confirmChoiceText.text = string.Format("手牌数量为{0}超过8，请选择要丢弃的手牌：", Player.Get().GetHandCards().Count);
		}
	}

	public void EndSelectCardToSpend()
	{
		Card.m_lstSelectCards.ForEach(card =>
		{
			// Restore position
			RectTransform rt = (RectTransform)card.m_thisInListView.gameObject.GetComponent<RectTransform>().parent;
			rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y - 30);

			card.Discard();
		});

		if(m_choiceMode == ConfirmButtonMode.DiscardExcessHandCards)
		{
			string log = "{0}因为手牌限制丢弃了如下手牌：";

			Card.m_lstSelectCards.ForEach(card =>
			{
				log += "<" + card.m_cardName + "> ";
			});

			GameLogic.Get().OutputGameLog(string.Format(log, Player.Get().m_investigatorCard.m_cardName));
		}

		Card.m_lstSelectCards.Clear();
		m_gameArea.SetActive(true);
		GameLogic.Get().m_cardClickMode = Card.CardClickMode.Flip;
		m_confirmChoiceText.gameObject.SetActive(false);
	}

	public void OnTargetDropdownChanged(Dropdown d)
	{
		// Option 0 is reserved
		if(d.value == 0 || Player.Get().GetCurrentAction() >= PlayerAction.NonStandardAction1)
		{
			return;
		}

		m_targetDropdown.gameObject.SetActive(false);

		if (Player.Get().GetCurrentAction() == PlayerAction.Fight)
		{
			OnPlayerFightEnemy(Player.Get().GetEnemyFromEngagedOrLocation(d.value - 1));
		}
		else if(Player.Get().GetCurrentAction() == PlayerAction.Evade)
		{
			OnPlayerEvadeEnemy(Player.Get().GetEnemyCards()[d.value - 1]);
		}
		else if(Player.Get().GetCurrentAction() == PlayerAction.Move)
		{
			string locName = d.options[d.value].text;
			var locList = GameLogic.Get().m_currentScenario.m_lstOtherLocations;

			for (int i = 0; i < locList.Count; ++i)
			{
				if (locList[i].m_cardName == locName)
				{
					GameLogic.Get().PlayerEnterLocation(locList[i].gameObject);
					break;
				}
			}
		}
		else if(Player.Get().GetCurrentAction() == PlayerAction.PlayCard)
		{
			string cardName = d.options[d.value].text;
			var cards = Player.Get().GetHandCards();

			for (int i = 0; i < cards.Count; ++i)
			{
				if (cards[i].m_cardName == cardName)
				{
					StartCoroutine(cards[i].PlayIt());
					break;
				}
			}
		}

		m_targetDropdown.value = 0;
	}

	public void ShowHighlightCardExclusive(Card card, bool bFlip, bool bDisableUI = true)
	{
		m_bConfirmModeEnd = false;

		if (bFlip)
		{
			card.FlipCard();
		}

		m_tempHighlightCard = card.gameObject;
		card.OnPointerEnter(null);
		card.m_image.raycastTarget = false;

		if (bDisableUI)
		{
			GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = false;
			GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = false;
		}
	}

	public void UpdateTargetDropdown(params object[] objects)
	{
		// ...................Seems like Unity's BUG.......................
		ScrollRect dropDownList = m_targetDropdown.GetComponentInChildren<ScrollRect>();
		if (dropDownList != null)
		{
			GameObject.Destroy(dropDownList.gameObject);
		}
		m_targetDropdown.ClearOptions();
		List<string> optionNames = new List<string>();

		switch(Player.Get().GetCurrentAction())
		{
			case PlayerAction.Move:
				{
					var destList = Player.Get().m_currentLocation.m_lstDestinations;

					optionNames.Add("请选择目的地...");
					destList.ForEach(dest => { optionNames.Add(dest.m_cardName); });
				}
				break;

			case PlayerAction.PlayCard:
				{
					optionNames.Add("请选择手牌...");
					var handCards = Player.Get().GetHandCards();

					handCards.ForEach(dest =>
					{
						if (Player.Get().CanPlayHandCard(dest))
						{
							optionNames.Add(dest.m_cardName);
						}
					});
				}
				break;

			case PlayerAction.Evade:
				{
					optionNames.Add("请选择目标...");
					var enemies = Player.Get().GetEnemyCards();

					enemies.ForEach(enemy => { optionNames.Add(enemy.m_cardName); });
				}
				break;

			case PlayerAction.Fight:
			case PlayerAction.BeatcopCardAction:
				{
					optionNames.Add("请选择目标...");
					var enemies = Player.Get().GetEnemyCards();

					enemies.ForEach(enemy => { optionNames.Add(enemy.m_cardName); });

					Player.Get().m_currentLocation.m_lstCardsAtHere.ForEach(card => 
					{
						if(card is EnemyCard)
						{
							optionNames.Add(card.m_cardName);
						}
					});
				}
				break;

			case PlayerAction.ReactiveEvent:
				{
					var cards = Player.Get().GetHandCards();

					optionNames.Add("是否要打出事件牌...");
					optionNames.Add("不打出");
					cards.ForEach(dest =>
					{
						if (dest.m_eventTiming == GameLogic.Get().m_currentTiming)
						{
							optionNames.Add(dest.m_cardName);
						}
					});
				}
				break;

			case PlayerAction.ReactiveAsset:
				{
					var cards = Player.Get().GetAssetAreaCards();

					optionNames.Add("是否要使用资产牌的技能...");
					optionNames.Add("不使用");
					cards.ForEach(dest =>
					{
						if (dest.m_eventTiming == GameLogic.Get().m_currentTiming)
						{
							optionNames.Add(dest.m_cardName);
						}
					});
				}
				break;

			case PlayerAction.AssignDamage:
				{
					optionNames.Add("请分配伤害...");
					AllyCard ally = (AllyCard)objects[1];
					int totalDamage = (int)objects[0];

					for (int i = 0; i <= ally.m_health && i <= totalDamage; ++i)
					{
						optionNames.Add(string.Format("你{0}点伤害，盟友{1}点伤害", totalDamage - i, i));
					}
				}
				break;

			case PlayerAction.AssignHorror:
				{
					optionNames.Add("请分配恐怖...");
					AllyCard ally = (AllyCard)objects[1];
					int totalDamage = (int)objects[0];

					for (int i = 0; i <= ally.m_sanity && i <= totalDamage; ++i)
					{
						optionNames.Add(string.Format("你{0}点恐怖，盟友{1}点恐怖", totalDamage - i, i));
					}
				}
				break;

			default:
				UnityEngine.Assertions.Assert.IsTrue(false, "Unknown player action in UpdateTargetDropdown()!!!");
				break;
		}

		m_targetDropdown.AddOptions(optionNames);
		m_targetDropdown.RefreshShownValue();
		m_targetDropdown.value = 0;
	}

	public void _OutputLog(string log)
	{
		StartCoroutine(_OutputLogCoroutine(log));
	}

	private IEnumerator _OutputLogCoroutine(string log)
	{
		m_gameLog.text += log;

		// Wait for one frame
		yield return null;
		m_logScrollBar.value = 0.001f;
	}

	public void _OnConfirmAssignDamage(int index)
	{
		StartCoroutine(_OnConfirmAssignDamageCoroutine(index));
	}

	private IEnumerator _OnConfirmAssignDamageCoroutine(int index)
	{
		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_targetDropdown.onValueChanged.RemoveListener(Player.Get().m_onAssignDamage);

		AllyCard ally = Player.Get().GetAssetCardInSlot(AssetSlot.Ally) as AllyCard;

		UnityEngine.Assertions.Assert.IsNotNull(ally, "Assert failed in Player.OnConfirmAssignDamage()!!!");

		int allyDamage = index - 1;
		int investigatorDamage = Player.Get().m_assignDamage - allyDamage;

		if (Player.Get().GetCurrentAction() == PlayerAction.AssignDamage)
		{
			ally.m_health -= allyDamage;
			Player.Get().m_health -= investigatorDamage;

			if (Player.Get().m_attacker)
			{
				GameLogic.Get().m_afterAssignDamageEvent.Invoke(Player.Get().m_attacker, investigatorDamage, allyDamage);

				yield return new WaitUntil(() => GameLogic.Get().m_currentTiming == EventTiming.None);
			}
		}
		else
		{
			ally.m_sanity -= allyDamage;
			Player.Get().m_sanity -= investigatorDamage;
		}

		if (ally.m_health <= 0 || ally.m_sanity <= 0)
		{
			ally.Discard();

			GameLogic.Get().OutputGameLog(string.Format("{0}的盟友<{1}>被打死了！\n", Player.Get().m_investigatorCard.m_cardName, ally.m_cardName));
		}

		ui.m_targetDropdown.gameObject.SetActive(false);
		ui.m_actionDropdown.gameObject.SetActive(GameLogic.Get().m_currentPhase == TurnPhase.InvestigationPhase);
		Player.Get().m_currentAction.Pop();
	}

	public void RemoveCardFromListView(CardListView list, Card card)
	{
		var holder = list.GetItemViewsHolderIfVisible(card.m_thisInListView.transform.parent as RectTransform);
		list.RemoveItemFrom(holder.ItemIndex, 1);
		card.m_thisInListView = card;
	}
}
