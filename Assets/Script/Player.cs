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

	private List<GameObject>	m_lstPlayerCards = new List<GameObject>();

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
	
	public void AddHandCard(GameObject go)
	{
		m_lstPlayerCards.Add(go);
	}

	public List<GameObject> GetHandCards()
	{
		return m_lstPlayerCards;
	}

	public void DecreaseHealth()
	{
		m_health -= 1;
	}

	public void DecreaseSanity()
	{
		m_sanity -= 1;
	}
}
