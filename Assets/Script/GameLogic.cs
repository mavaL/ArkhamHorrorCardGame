using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;


public enum GameDifficulty
{
    Easy,
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



public class GameLogic
{
    public static GameLogic s_gameLogic = null;

	public GameLogic()
	{
		UnityEngine.Random.InitState((int)System.DateTime.Now.ToUniversalTime().ToBinary());

		m_core_scenarios[0] = new core_gathering();
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
	public scenario_base[]	m_core_scenarios = new scenario_base[3];
	public Card.CardClickMode	m_cardClickMode = Card.CardClickMode.Flip;
	public Text				m_logText;

	public static void Swap<T>(ref T a, ref T b)
	{
		T t = a;
		a = b;
		b = t;
	}

	public static void DockCard(GameObject go, GameObject parent)
	{
		go.transform.SetParent(parent.transform);
		go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
	}

	public static void PlayerEnterLocation(GameObject locationGO)
	{
		Player.Get().m_currentLocation = locationGO.GetComponent<LocationCard>();

		if(Player.Get().m_currentLocation == null)
		{
			Debug.LogError("locationGO.GetComponent<LocationCard>() is null in PlayerEnterLocation()!!");
		}

		Player.Get().m_playerToken.transform.SetParent(locationGO.transform);

		var rt = Player.Get().m_playerToken.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.sizeDelta.x/2, rt.sizeDelta.y/2);
		rt.localScale = new Vector3(1, 1, 1);
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

	public static GameObject DrawCard(List<GameObject> cards)
	{
		if(cards.Count == 0)
		{
			return null;
		}

		var card = cards[0];
		cards.RemoveAt(0);
		card.SetActive(true);

		return card;
	}

	public void StartScenario()
	{
		m_core_scenarios[0].StartScenario();
	}

	public void Update()
	{
		m_core_scenarios[0].ShowPlayInfo();
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
				chaosResult = m_core_scenarios[0].GetChaosTokenEffect(chaosToken);
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
			OutputGameLog("调查成功！\n");
		}
		else
		{
			// Failed..
			OutputGameLog("调查失败！\n");
			m_core_scenarios[0].AfterSkillTestFailed(chaosToken);
		}

        if(chaosToken == ChaosBag.ChaosTokenType.ElderSign)
        {
            if(Player.Get().m_investigatorCard.m_afterElderSignEvent.methodName != null)
            {
                Player.Get().m_investigatorCard.m_afterElderSignEvent.Invoke(bSucceed);
            }
        }
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
