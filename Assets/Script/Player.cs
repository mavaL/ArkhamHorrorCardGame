using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	private int					m_actionUsed = 0;
	public int					m_totalActions = 3;

    // Hand cards
	private List<PlayerCard>	m_lstPlayerCards = new List<PlayerCard>();
    // Asset cards played
    private List<PlayerCard>    m_lstAssetCards = new List<PlayerCard>();
	// Engaged enemies
	private List<EnemyCard>		m_lstEnemyCards = new List<EnemyCard>();

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
			m_lstPlayerCards.Add(go.GetComponent<PlayerCard>());
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

	public void ActionDone()
	{
		m_actionUsed += 1;

		if(ActionLeft() == 0)
		{
			GameLogic.Get().m_mainGameUI.EnterEnemyPhase();
		}
	}

	public void ResetAction()
	{
		m_actionUsed = 0;
	}

	public int ActionLeft()
	{
		return m_totalActions - m_actionUsed;
	}

	public void AddEngagedEnemy(EnemyCard enemy)
	{
		m_lstEnemyCards.Add(enemy);
		GameLogic.Get().OutputGameLog(string.Format("{0}与<{1}>发生了交战！\n", m_investigatorCard.m_cardName, enemy.m_cardName));
	}
}
