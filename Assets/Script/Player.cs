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

    // Hand cards
	private List<GameObject>	m_lstPlayerCards = new List<GameObject>();
    // Asset cards played
    private List<PlayerCard>    m_lstAssetCards = new List<PlayerCard>();

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
		m_lstPlayerCards.Add(go);
	}

	public List<GameObject> GetHandCards()
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

	public void DecreaseHealth()
	{
		m_health -= 1;
	}

	public void DecreaseSanity()
	{
		m_sanity -= 1;
	}

    public int HowManySanityIsLost()
    {
        return m_investigatorCard.m_sanity - m_sanity;
    }
}
