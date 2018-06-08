using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionDoneEvent : UnityEvent<PlayerAction> { }

// Value should correspond with ActionDropdown control
public enum PlayerAction
{
	Move = 1,
	Investigate,
	Fight,
	Evade,
	DrawOneCard,
	GainOneResource,
	PlayCard,
	OtherAction
}

public class Player
{
	public static Player s_player = null;

	public int					m_currentScenario = -1;
	public InvestigatorCard		m_investigatorCard;
	public GameObject			m_playerToken;
	public LocationCard			m_currentLocation;

	private int					m_health = -1;
	private int					m_sanity = -1;
	public int					m_resources = -1;
	public Faction				m_faction;
	public int					m_clues;
	public int					m_actionUsed = 0;
	public int					m_totalActions = 3;
	public ActionDoneEvent		m_actionDoneEvent = new ActionDoneEvent();
	public List<PlayerAction>	m_actionRecord = new List<PlayerAction>();

	// Hand cards
	private List<PlayerCard>	m_lstPlayerCards = new List<PlayerCard>();
    // Asset cards played
    private List<PlayerCard>    m_lstAssetCards = new List<PlayerCard>();
	// Engaged enemies
	private List<EnemyCard>		m_lstEnemyCards = new List<EnemyCard>();
	// Engaged treachery
	private List<TreacheryCard> m_lstTreacheryCards = new List<TreacheryCard>();

	public Player()
	{
		m_resources = 5;
	}

	static public Player Get()
	{
		if (s_player == null)
		{
			s_player = new Player();
		}

		return s_player;
	}

    public void InitPlayer(InvestigatorCard card)
    {
        m_investigatorCard = card;
        // 如果有父对象被摧毁，其子对象也会被摧毁
        m_investigatorCard.transform.SetParent(null);
        GameObject.DontDestroyOnLoad(m_investigatorCard);

        m_health = m_investigatorCard.m_health;
        m_sanity = m_investigatorCard.m_sanity;
    }
	
	public void AddHandCard(GameObject go)
	{
		if(go != null)
		{
			PlayerCard pc = go.GetComponent<PlayerCard>();
			pc.m_currentLocation = null;

			m_lstPlayerCards.Add(pc);
			go.SetActive(true);
		}
	}

	public void DiscardHandCard(PlayerCard card)
	{
		if(card != null)
		{
			m_lstPlayerCards.Remove(card);
		}
	}

	public List<PlayerCard> GetHandCards()
	{
		return m_lstPlayerCards;
	}

	public List<EnemyCard> GetEnemyCards()
	{
		return m_lstEnemyCards;
	}

	public List<TreacheryCard> GetTreacheryCards()
	{
		return m_lstTreacheryCards;
	}

	public bool IsAssetCardInPlay(string cardName)
    {
        for(int i=0; i<m_lstAssetCards.Count; ++i)
        {
            if(m_lstAssetCards[i].m_cardName == cardName)
            {
                return true;
            }
        }
        return false;
    }

	public bool CanPlayHandCard()
	{
		foreach(var card in m_lstPlayerCards)
		{
			if(card.m_lstKeywords.Contains(Card.Keyword.Asset) ||
				card.m_lstKeywords.Contains(Card.Keyword.Event))
			{
				return true;
			}
		}
		return false;
	}

	public bool ChooseAndDiscardAssetCard()
	{
		// TODO: Asset card impl
		return false;
	}

	public int GetHp()
	{
		return m_health;
	}

	public int GetSan()
	{
		return m_sanity;
	}

	public void DecreaseHealth(int mount = 1)
	{
		m_health -= mount;
	}

	public void DecreaseSanity(int mount = 1)
	{
		m_sanity -= mount;
	}

    public int HowManySanityIsLost()
    {
        return m_investigatorCard.m_sanity - m_sanity;
    }

	public void ActionDone(PlayerAction action)
	{
		m_actionUsed += 1;
		m_actionDoneEvent.Invoke(action);
		m_actionRecord.Add(action);

		if (ActionLeft() == 0)
		{
			GameLogic.Get().m_mainGameUI.EnterEnemyPhase();
		}
		else
		{
			GameLogic.Get().m_mainGameUI.ResetActionDropdown();
		}
	}

	public void ResetAction()
	{
		m_actionUsed = 0;
		m_actionRecord.Clear();
	}

	public int ActionLeft()
	{
		return m_totalActions - m_actionUsed;
	}

	public void AddEngagedEnemy(EnemyCard enemy)
	{
		enemy.m_engaged = true;
		m_lstEnemyCards.Add(enemy);
		GameLogic.m_lstUnengagedEnemyCards.Remove(enemy);
		enemy.gameObject.SetActive(true);

		GameLogic.Get().m_mainGameUI.OnPlayerThreatAreaChnaged();
		GameLogic.Get().OutputGameLog(string.Format("{0}与<{1}>发生了交战！\n", m_investigatorCard.m_cardName, enemy.m_cardName));
	}

	public void AddTreachery(TreacheryCard treachery)
	{
		m_lstTreacheryCards.Add(treachery);
		treachery.gameObject.SetActive(true);

		GameLogic.Get().m_mainGameUI.OnPlayerThreatAreaChnaged();
	}

	public void RemoveEngagedEnemy(EnemyCard enemy)
	{
		enemy.gameObject.SetActive(false);
		m_lstEnemyCards.Remove(enemy);
		GameLogic.Get().m_mainGameUI.OnPlayerThreatAreaChnaged();
	}

	public int GetSkillValueByType(SkillType skill)
	{
		switch (skill)
		{
			case SkillType.Agility: return m_investigatorCard.m_agility;
			case SkillType.Combat: return m_investigatorCard.m_combat;
			case SkillType.Intellect: return m_investigatorCard.m_intellect;
			case SkillType.Willpower: return m_investigatorCard.m_willPower;
		}

		return -999;
	}

	public void ResolveEngagedEnemyAttack()
	{
		foreach(var enemy in m_lstEnemyCards)
		{
			if(!enemy.m_exhausted)
			{
				m_health -= enemy.m_damage;
				m_sanity -= enemy.m_horror;

				GameLogic.Get().OutputGameLog(string.Format("{0}被<{1}>攻击，受到：{2}点伤害，{3}点恐怖！\n", m_investigatorCard.m_cardName, enemy.m_cardName, enemy.m_damage, enemy.m_horror));

				enemy.m_exhausted = true;
				GameLogic.m_lstExhaustedCards.Add(enemy);
			}
		}
	}
}
