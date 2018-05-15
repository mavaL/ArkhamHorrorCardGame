using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public int					m_resources = -1;
	public Faction				m_faction;
	public int					m_currentScenario = -1;

	private List<GameObject>	m_lstPlayerCards = new List<GameObject>();

	// Use this for initialization
	void Start ()
	{
		m_resources = 5;
	}
	
	public void AddHandCard(GameObject go)
	{
		m_lstPlayerCards.Add(go);
	}
}
