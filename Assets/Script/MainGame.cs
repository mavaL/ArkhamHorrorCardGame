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
	public Button m_confirmChoiceBtn;
	public Button m_useLocationAbilityBtn;

	public Dropdown m_targetDropdown;
	public Dropdown m_choiceDropdown;
	public Dropdown m_actionDropdown;
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
		ParleyWithLita
	}
	[System.NonSerialized]
	public ConfirmButtonMode m_choiceMode = ConfirmButtonMode.None;
	[System.NonSerialized]
	public List<UnityEvent>	m_lstChoiceEvent = new List<UnityEvent>();
	// Used by LocationEvent
	[System.NonSerialized]
	public List<PlayerCard> m_lstCardChoice = new List<PlayerCard>(0);
	[System.NonSerialized]
	public GameObject		m_tempHighlightCard;
	public bool				m_bConfirmModeEnd { get; set; }
	public UnityEvent		m_investigatePhaseBeginEvent { get; set; } = new UnityEvent();
	public UnityEvent		m_enemyPhaseBeginEvent { get; set; } = new UnityEvent();
	public UnityEvent		m_upkeepPhaseBeginEvent { get; set; } = new UnityEvent();
	public UnityEvent		m_mythosPhaseBeginEvent { get; set; } = new UnityEvent();
	public UnityEvent		m_roundEndEvent { get; set; } = new UnityEvent();
	private UnityAction<int> m_onPlayReactiveEvent;

	string[] m_roland_def_cards =
	{
		"Guardian/core_guardian_dodge",
		"Neutral/core_roland_dot38_special",
		"Neutral/core_cover_up",
		"Guardian/core_guardian_dot45_automatic",
		"Guardian/core_guardian_physical_training",
		"Guardian/core_guardian_beat_cop",
		"Guardian/core_guardian_first_aid",
		"Guardian/core_guardian_machete",
		"Guardian/core_guardian_dog",
		"Guardian/core_guardian_evidence",
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
	}

	// Use this for initialization
	void Start()
	{
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
		// Only process standard actions
		if (d.value == 0 || d.value >= (int)PlayerAction.NonStandardAction1)
		{
			return;
		}

		m_actionDropdown.gameObject.SetActive(false);
		Player.Get().m_currentAction = (PlayerAction)d.value;

		switch (Player.Get().m_currentAction)
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
		}
	}

	public void InvestigateCurrentLocation()
	{
		m_tempHighlightCard = Player.Get().m_currentLocation.gameObject;
		m_confirmChoiceBtn.gameObject.SetActive(true);
		m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;

		BeginSelectCardToSpend();
		Player.Get().ActionDone(PlayerAction.Investigate);
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
		// Display player hand cards
		var cards = Player.Get().GetHandCards();

		for (int i = 0; i < cards.Count; ++i)
		{
			GameLogic.DockCard(cards[i].gameObject, m_playerCardArea[i]);
		}

		/// TODO: Threaten area and player hand-cards area should use scroll-bar control
		// Display player engaged treachery
		var treachery = Player.Get().GetTreacheryCards();

		for (int i = 0; i < treachery.Count; ++i)
		{
			GameLogic.DockCard(treachery[i].gameObject, m_threatArea[i]);
		}

		// Display player engaged enemy
		var enemies = Player.Get().GetEnemyCards();

		for (int i = 0; i < enemies.Count; ++i)
		{
			GameLogic.DockCard(enemies[i].gameObject, m_threatArea[i + treachery.Count]);
		}

		m_advanceActBtn.interactable = GameLogic.Get().IsClueEnoughToAdvanceAct();

		GameLogic.Get().Update();
	}

	public void EnterEnemyPhase()
	{
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
		if (enemies.Count > 0)
		{
			foreach (var enemy in enemies)
			{
				if (enemy.IsKeywordContain(Card.Keyword.Hunter))
				{
					enemy.HunterMoveToNearestInvestigator();
				}
			}
		}

		// 2. Resolve engaged enemy attacks
		StartCoroutine(OnAllEnemyAttack());
	}

	IEnumerator OnAllEnemyAttack()
	{
		var enemies = Player.Get().GetEnemyCards();

		foreach (var enemy in enemies)
		{
			if (Player.Get().CanPlayEvent(EventTiming.EnemyAttack))
			{
				GameLogic.Get().m_currentTiming = EventTiming.EnemyAttack;
				Player.Get().m_currentAction = PlayerAction.ReactiveEvent;
				m_targetDropdown.gameObject.SetActive(true);
				UpdateTargetDropdown();

				GameLogic.Get().ShowHighlightCardExclusive(enemy, false, false);

				m_targetDropdown.onValueChanged.AddListener(m_onPlayReactiveEvent);

				yield return new WaitUntil(()=> GameLogic.Get().m_currentTiming == EventTiming.None);

				Player.Get().m_currentAction = PlayerAction.None;
			}

			Player.Get().ResolveEngagedEnemyAttack(enemy);
		}

		m_UpkeepPhaseBtn.gameObject.SetActive(true);
	}

	public void OnPlayReactiveEvent(int index)
	{
		if(index > 1)
		{
			var cards = Player.Get().GetHandCards();

			for (int i = 0; i < cards.Count; ++i)
			{
				if (cards[i].m_cardName == m_targetDropdown.options[index].text)
				{
					cards[i].GetComponent<PlayerCardLogic>().OnReveal(m_tempHighlightCard.GetComponent<Card>());
					break;
				}
			}		
		}

		GameLogic.Get().m_currentTiming = EventTiming.None;
		m_targetDropdown.onValueChanged.RemoveListener(m_onPlayReactiveEvent);
		m_tempHighlightCard.GetComponent<Card>().OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
		m_tempHighlightCard = null;
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

		m_UpkeepPhaseBtn.gameObject.SetActive(false);
		m_MythosPhaseBtn.gameObject.SetActive(true);

		m_roundEndEvent.Invoke();
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
			m_bConfirmModeEnd = true;
		}
		else if(type == 2)
		{
			m_confirmAgendaResultBtn.gameObject.SetActive(false);

			var scenario = GameLogic.Get().m_currentScenario;
			scenario.m_currentAgenda.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));

			scenario.AdvanceAgenda();
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
				m_tempHighlightCard.GetComponent<Card>().OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
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

			card.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
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

				card.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
				m_tempHighlightCard = null;
			}
			else if(card is TreacheryCard)
			{
				TreacheryCard treacheryCard = card as TreacheryCard;
				treacheryCard.m_onRevealEvent.Invoke();

				Treachery treachery = card.GetComponent<Treachery>();

				if (treachery != null)
				{
					treachery.OnReveal(treacheryCard);
					card.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
					m_tempHighlightCard = null;
				}	
				else if(treacheryCard.m_skillTestEvent.GetPersistentEventCount() > 0)
				{
					GameLogic.Get().m_mainGameUI.m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;
					GameLogic.Get().m_mainGameUI.BeginSelectCardToSpend();
				}
			}
		}
		else if(m_choiceMode == ConfirmButtonMode.SkillTest)
		{
			Card card = m_tempHighlightCard.GetComponent<Card>();
			card.OnSkillTest();

			card.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
			m_tempHighlightCard = null;
			EndSelectCardToSpend();
		}

		m_bConfirmModeEnd = true;
	}

	public void OnPlayerFightEnemy(EnemyCard enemy)
	{
		m_confirmChoiceBtn.gameObject.SetActive(true);
		m_tempHighlightCard = enemy.gameObject;
		m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;
		BeginSelectCardToSpend();
	}

	public void OnPlayerEvadeEnemy(EnemyCard enemy)
	{
		m_confirmChoiceBtn.gameObject.SetActive(true);
		m_tempHighlightCard = enemy.gameObject;
		m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;
		BeginSelectCardToSpend();
	}

	public void ResetActionDropdown()
	{
		m_actionDropdown.gameObject.SetActive(true);
		m_actionDropdown.value = 0;
		Player.Get().m_currentAction = PlayerAction.None;

		// ...................Seems like Unity's BUG.......................
		ScrollRect dropDownList = m_actionDropdown.GetComponentInChildren<ScrollRect>();
		if (dropDownList != null)
		{
			Destroy(dropDownList.gameObject);
		}
	}

	public void BeginSelectCardToSpend()
	{
		m_confirmChoiceBtn.gameObject.SetActive(true);	
		m_confirmSkillTestText.gameObject.SetActive(true);
		m_gameArea.SetActive(false);
		GameLogic.Get().m_cardClickMode = Card.CardClickMode.MultiSelect;
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

	public void OnTargetDropdownChanged(Dropdown d)
	{
		// Option 0 is reserved
		if(d.value == 0 || Player.Get().m_currentAction >= PlayerAction.NonStandardAction1)
		{
			return;
		}

		m_targetDropdown.gameObject.SetActive(false);

		if (Player.Get().m_currentAction == PlayerAction.Fight)
		{
			OnPlayerFightEnemy(Player.Get().GetEnemyCards()[d.value - 1]);
		}
		else if(Player.Get().m_currentAction == PlayerAction.Evade)
		{
			OnPlayerEvadeEnemy(Player.Get().GetEnemyCards()[d.value - 1]);
		}
		else if(Player.Get().m_currentAction == PlayerAction.Move)
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
		else if(Player.Get().m_currentAction == PlayerAction.PlayCard)
		{
			string cardName = d.options[d.value].text;
			var cards = Player.Get().GetHandCards();

			for (int i = 0; i < cards.Count; ++i)
			{
				if (cards[i].m_cardName == cardName)
				{
					cards[i].PlayIt();
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
		card.OnPointerEnter(new UnityEngine.EventSystems.BaseEventData(null));
		card.m_image.raycastTarget = false;

		if (bDisableUI)
		{
			GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = false;
			GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = false;
		}
	}

	public void UpdateTargetDropdown()
	{
		// ...................Seems like Unity's BUG.......................
		ScrollRect dropDownList = m_targetDropdown.GetComponentInChildren<ScrollRect>();
		if (dropDownList != null)
		{
			GameObject.Destroy(dropDownList.gameObject);
		}
		m_targetDropdown.ClearOptions();
		List<string> optionNames = new List<string>();

		if (Player.Get().m_currentAction == PlayerAction.Move)
		{
			var destList = Player.Get().m_currentLocation.m_lstDestinations;

			optionNames.Add("请选择目的地...");
			destList.ForEach(dest => { optionNames.Add(dest.m_cardName); });
		}
		else if(Player.Get().m_currentAction == PlayerAction.PlayCard)
		{
			optionNames.Add("请选择手牌...");
			var handCards = Player.Get().GetHandCards();

			handCards.ForEach(dest => 
			{
				if (dest.m_lstKeywords.Contains(Card.Keyword.Asset) || dest.m_lstKeywords.Contains(Card.Keyword.Event))
				{
					optionNames.Add(dest.m_cardName);
				}
			});
		}
		else if(Player.Get().m_currentAction == PlayerAction.Fight ||
			Player.Get().m_currentAction == PlayerAction.Evade)
		{
			optionNames.Add("请选择目标...");
			var enemies = Player.Get().GetEnemyCards();

			enemies.ForEach(enemy => { optionNames.Add(enemy.m_cardName); });
		}
		else if(Player.Get().m_currentAction == PlayerAction.ReactiveEvent)
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

		m_targetDropdown.AddOptions(optionNames);
		m_targetDropdown.RefreshShownValue();
		m_targetDropdown.value = 0;
	}
}
