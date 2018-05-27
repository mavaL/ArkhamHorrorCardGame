using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;


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
	UpkeepPhase
}

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
	public Text				m_logText;
	public TurnPhase		m_currentPhase;
	public ChaosBag			m_chaosBag = new ChaosBag();
	public MainGame			m_mainGameUI;

	public List<GameObject> m_lstPlayerCards = new List<GameObject>();
	public List<GameObject> m_lstEncounterCards = new List<GameObject>();
	public List<GameObject> m_lstDiscardPlayerCards = new List<GameObject>();

	public static void Swap<T>(ref T a, ref T b)
	{
		T t = a;
		a = b;
		b = t;
	}

	public static void DockCard(GameObject go, GameObject parent, bool bWorldPosStays = true)
	{
		go.transform.SetParent(parent.transform, bWorldPosStays);
		go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
	}

	public void PlayerEnterLocation(GameObject locationGO)
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

		// Update movement destination list
		var destList = Player.Get().m_currentLocation.m_lstDestinations;
		if (destList.Count > 0)
		{
			m_mainGameUI.m_movementDropdown.ClearOptions();

			List<string> destNames = new List<string>();
			destNames.Add("移动到...");
			destList.ForEach(dest => { destNames.Add(dest.m_cardName); });
			m_mainGameUI.m_movementDropdown.AddOptions(destNames);

			m_mainGameUI.m_movementDropdown.value = 0;
			m_mainGameUI.m_movementDropdown.interactable = true;
		}
		else
		{
			m_mainGameUI.m_movementDropdown.interactable = false;
		}
	}

	public void ShowHighlightCardExclusive(Card card, bool bFlip)
	{
		if(bFlip)
		{
			card.FlipCard();
		}
		
		card.OnPointerEnter(new UnityEngine.EventSystems.BaseEventData(null));
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = false;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = false;
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

	public void Update()
	{
		m_currentScenario.ShowPlayInfo();
	}

	public void InvestigateCurrentLocation(ChaosBag bag)
	{
		bool bAutoFailed = false;
		int chaosResult = 0;
		var chaosToken = bag.GetResult();

		switch(chaosToken)
		{
			case ChaosBag.ChaosTokenType.Tentacle:
				bAutoFailed = true;    // Auto failed...
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

		int shroudValue = Player.Get().m_currentLocation.m_shroud;
		int skillIconValue = GetSkillIconNumInSelectCards(PlayerCard.SkillIcon.Intellect);
		int playerSkillValue = Player.Get().m_investigatorCard.m_intellect;
		int finalValue = skillIconValue + chaosResult + playerSkillValue - shroudValue;

		OutputGameLog(string.Format("知识：{0}\n打出技能图标：{1}\n混沌标记（{2}）：{3}\n笼罩：{4}\n最终结果：{5}\n",
			playerSkillValue,
			skillIconValue,
			ChaosBag.GetChaosTokenName(chaosToken),
			chaosResult,
			shroudValue,
			finalValue));

        bool bSucceed = !bAutoFailed && finalValue >= 0;

		if (bSucceed)
		{
			// Succeed!
			Player.Get().m_clues += 1;
			Player.Get().m_currentLocation.m_clues -= 1;
			OutputGameLog("调查成功！\n");
		}
		else
		{
			// Failed..
			OutputGameLog("调查失败！\n");
			m_currentScenario.AfterSkillTestFailed(chaosToken);
		}

        if(chaosToken == ChaosBag.ChaosTokenType.ElderSign)
        {
            if(Player.Get().m_investigatorCard.m_afterElderSignEvent.GetPersistentEventCount() > 0)
            {
                Player.Get().m_investigatorCard.m_afterElderSignEvent.Invoke(bSucceed);
            }
        }

		Player.Get().m_actionUsed += 1;
	}

	public int GetSkillIconNumInSelectCards(PlayerCard.SkillIcon type)
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

			if (pc.m_skillIcons.ContainsKey(PlayerCard.SkillIcon.Wild))
			{
				result += pc.m_skillIcons[PlayerCard.SkillIcon.Wild];
			}
		}
		return result;
	}

	public void OutputGameLog(string log)
	{
		m_logText.text += log;
	}
}
