using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.Events;

public enum GameDifficulty
{
    Easy = 0,
    Normal,
    Hard,
    Expert
}

public enum Faction
{
	Guardian,
	Survivor,
	Mystic,
	Rogue,
	Seeker
}

public enum TurnPhase
{
	MythosPhase,
	InvestigationPhase,
	EnemyPhase,
	UpkeepPhase,
	ScenarioEnd
}

// Param 1: attacker causes this damage
// Param 2: damage assign to investigator
// Param 3: damage assign to ally
public class AfterAssignDamageEvent : UnityEvent<EnemyCard, int, int> { }

public class GameLogic
{
    public static GameLogic s_gameLogic = null;

	public GameLogic()
	{
		UnityEngine.Random.InitState((int)System.DateTime.Now.ToUniversalTime().ToBinary());
	}

	static public GameLogic Get()
    {
        if(s_gameLogic == null)
        {
            s_gameLogic = new GameLogic();
        }

        return s_gameLogic;
    }

	public GameDifficulty	m_difficulty = GameDifficulty.Normal;
	public scenario_base	m_currentScenario;
	public Card.CardClickMode	m_cardClickMode = Card.CardClickMode.Flip;
	public TurnPhase		m_currentPhase;
	public EventTiming		m_currentTiming = EventTiming.None;
	public ChaosBag			m_chaosBag = new ChaosBag();
	public MainGame			m_mainGameUI;
	public bool				m_bCanAdvanceAct = false;

	public List<GameObject>			m_lstPlayerCards = new List<GameObject>();
	public List<GameObject>			m_lstDiscardEncounterCards = new List<GameObject>();
	public List<GameObject>			m_lstDiscardPlayerCards = new List<GameObject>();
	static public List<Card>		m_lstExhaustedCards = new List<Card>();
	static public List<EnemyCard>	m_lstUnengagedEnemyCards = new List<EnemyCard>();
	public UnityEvent				m_enemyAttackEvent { get; set; } = new UnityEvent();
	public AfterAssignDamageEvent	m_afterAssignDamageEvent { get; set; } = new AfterAssignDamageEvent();
	public UnityEvent				m_afterEnemyDamagedEvent { get; set; } = new UnityEvent();

	public static void Swap<T>(ref T a, ref T b)
	{
		T t = a;
		a = b;
		b = t;
	}

	public static void DockCard(GameObject go, GameObject parent, float yOffset = 0, bool bWorldPosStays = true, bool bDockLocation = false)
	{
		if (bDockLocation)
		{
			var destination = parent.GetComponent<LocationCard>();
			var card = go.GetComponent<Card>();

			card.m_currentLocation = destination;
			destination.m_lstCardsAtHere.Add(card);

			go.transform.SetParent(parent.transform.parent, bWorldPosStays);
			go.transform.SetAsFirstSibling();

			RectTransform rt = parent.GetComponent<RectTransform>();
			go.GetComponent<RectTransform>().anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + destination.m_lstCardsAtHere.Count * yOffset);
		}
		else
		{
			go.transform.SetParent(parent.transform, bWorldPosStays);
			go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		}
		go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);	
	}

	public void PlayerEnterLocation(GameObject locationGO, bool bUseAction = true)
	{
		Player.Get().m_currentLocation = locationGO.GetComponent<LocationCard>();

		// First time visit
		if (!Player.Get().m_currentLocation.m_isVisit)
		{
			ShowHighlightCardExclusive(Player.Get().m_currentLocation, false);

			m_mainGameUI.m_confirmEnterLocationBtn.gameObject.SetActive(true);
		}
		else
		{
			Player.Get().m_currentLocation.EnterLocation();
		}

		if (bUseAction)
		{
			Player.Get().ActionDone(PlayerAction.Move);
		}
	}

	public void ShowHighlightCardExclusive(Card card, bool bFlip, bool bDisableUI = true)
	{
		m_mainGameUI.ShowHighlightCardExclusive(card, bFlip, bDisableUI);
	}

	public static void Shuffle(List<GameObject> cards)
	{
		for(int i=0; i<cards.Count; ++i)
		{
			int newPos = Random.Range(0, cards.Count-1);
			var t = cards[i];
			cards[i] = cards[newPos];
			cards[newPos] = t;
		}
	}

	public void InitChaosBag()
	{
		m_chaosBag.Init(m_difficulty);
	}

	public GameObject DrawPlayerCard()
	{
		if(m_lstPlayerCards.Count == 0)
		{
			return null;
		}

		var card = m_lstPlayerCards[0];
		m_lstPlayerCards.RemoveAt(0);
		card.SetActive(true);

		return card;
	}

	public GameObject DrawEncounterCard()
	{
		// If encounter card pile is run out, shuffle the discard pile to form a new pile
		if (m_currentScenario.m_lstEncounterCards.Count == 0)
		{
			m_currentScenario.m_lstEncounterCards.AddRange(m_lstDiscardEncounterCards);
			Shuffle(m_currentScenario.m_lstEncounterCards);
			m_lstDiscardEncounterCards.Clear();
		}

		var card = m_currentScenario.m_lstEncounterCards[0];
		m_currentScenario.m_lstEncounterCards.RemoveAt(0);
		card.SetActive(true);

		return card;
	}

	public void Update()
	{
		m_currentScenario.ShowPlayInfo();
	}

	public int SkillTest(SkillType skill, int AgainstValue, out ChaosBag.ChaosTokenType chaosToken)
	{
		int chaosResult = 0;
		int skillIconValue = GetSkillIconNumInSelectCards(skill);
		int playerSkillValue = Player.Get().GetSkillValueByType(skill);
		chaosToken = m_chaosBag.GetResult();

		switch (chaosToken)
		{
			case ChaosBag.ChaosTokenType.Tentacle:
				// Auto failed...
				chaosResult = -skillIconValue - playerSkillValue;
				break;
			case ChaosBag.ChaosTokenType.ElderSign:
				chaosResult = Player.Get().m_investigatorCard.m_investigatorAbility.Invoke();
				break;
			case ChaosBag.ChaosTokenType.Zero:
			case ChaosBag.ChaosTokenType.Add_1:
			case ChaosBag.ChaosTokenType.Substract_1:
			case ChaosBag.ChaosTokenType.Substract_2:
			case ChaosBag.ChaosTokenType.Substract_3:
			case ChaosBag.ChaosTokenType.Substract_4:
			case ChaosBag.ChaosTokenType.Substract_5:
			case ChaosBag.ChaosTokenType.Substract_6:
			case ChaosBag.ChaosTokenType.Substract_7:
			case ChaosBag.ChaosTokenType.Substract_8:
				chaosResult = (int)chaosToken;
				break;
			default:
				chaosResult = m_currentScenario.GetChaosTokenEffect(chaosToken);
				break;
		}

		
		int finalValue = skillIconValue + chaosResult + playerSkillValue - AgainstValue;

		OutputGameLog(string.Format("技能检定如下：\n对应技能：<color=green>{0}</color>\n打出技能图标：<color=green>{1}</color>\n混沌标记<{2}>：<color=orange>{3}</color>\n检定值：<color=red>{4}</color>\n最终结果：{5}\n",
			playerSkillValue,
			skillIconValue,
			ChaosBag.GetChaosTokenName(chaosToken),
			chaosResult,
			-AgainstValue,
			finalValue));

		return finalValue;
	}

	public void AfterSkillTest(bool bSucceed, ChaosBag.ChaosTokenType chaosToken)
	{
		if(!bSucceed)
		{
			m_currentScenario.AfterSkillTestFailed(chaosToken);
		}

		if (chaosToken == ChaosBag.ChaosTokenType.ElderSign)
		{
			if (Player.Get().m_investigatorCard.m_afterElderSignEvent.GetPersistentEventCount() > 0)
			{
				Player.Get().m_investigatorCard.m_afterElderSignEvent.Invoke(bSucceed);
			}
		}
	}

	public int GetSkillIconNumInSelectCards(SkillType type)
	{
		int result = 0;

		for(int i=0; i<Card.m_lstSelectCards.Count; ++i)
		{
			PlayerCard pc = (PlayerCard)Card.m_lstSelectCards[i];
			Assert.IsTrue(pc != null, "PlayerCard is null in GetSkillIconNumInSelectCards()!!");

			if(pc.m_skillIcons.ContainsKey(type))
			{
				result += pc.m_skillIcons[type];
			}

			if (pc.m_skillIcons.ContainsKey(SkillType.Wild))
			{
				result += pc.m_skillIcons[SkillType.Wild];
			}
		}
		return result;
	}

	public void OutputGameLog(string log)
	{
		m_mainGameUI._OutputLog(log);
	}

	public bool CanAdvanceAct()
	{
		bool bEnoughClues = m_currentScenario.m_currentAct.m_currentToken + Player.Get().m_clues >= m_currentScenario.m_currentAct.m_tokenToAdvance;
		return m_bCanAdvanceAct || bEnoughClues;
	}

	// Just reveal, not enter it
	public void RevealLocation(LocationCard card)
	{
		m_mainGameUI.m_tempHighlightCard = card.gameObject;

		ShowHighlightCardExclusive(card, true);

		m_mainGameUI.m_confirmChoiceBtn.gameObject.SetActive(true);
		m_mainGameUI.m_choiceMode = MainGame.ConfirmButtonMode.RevealCard;
		card.m_isVisit = true;
	}

	public void SpawnAtLocation(Card card, LocationCard destination, bool bHighlight)
	{
		if(bHighlight)
		{
			m_mainGameUI.m_tempHighlightCard = card.gameObject;
			ShowHighlightCardExclusive(card, false);
			m_mainGameUI.m_confirmChoiceBtn.gameObject.SetActive(true);
			m_mainGameUI.m_choiceMode = MainGame.ConfirmButtonMode.RevealCard;
		}
		
		DockCard(card.gameObject, destination.gameObject, 300, true, true);

		card.OnSpawnAtLocation(destination);
	}

	public bool IsAnyEnemyToFightWith()
	{
		bool b1 = false;
		for (int i = 0; i < Player.Get().m_currentLocation.m_lstCardsAtHere.Count; ++i)
		{
			Card card = Player.Get().m_currentLocation.m_lstCardsAtHere[i];
			if (card is EnemyCard)
			{
				b1 = true;
				break;
			}
		}

		return b1 || Player.Get().GetEnemyCards().Count > 0;
	}
}
