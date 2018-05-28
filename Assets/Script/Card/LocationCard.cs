using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LocationCard : Card
{
	public int					m_clues = 0;
	public int					m_shroud = 0;
	public List<LocationCard>	m_lstDestinations;
	[System.NonSerialized]
	public List<EnemyCard>		m_lstEnemies = new List<EnemyCard>();
	[System.NonSerialized]
	public bool					m_isVisit = false;

	[System.Serializable]
	public class LocationCallback : UnityEvent<LocationCard> { }
	public LocationCallback		m_locationCallback;

	public void EnterLocation()
	{
		string log;

		if(!m_isVisit)
		{
			m_canFlip = true;
			FlipCard();
			m_isVisit = true;

			log = string.Format("{0}发现了{1}\n", Player.Get().m_investigatorCard.m_cardName, m_cardName);
		}
		else
		{
			log = string.Format("{0}移动到了{1}\n", Player.Get().m_investigatorCard.m_cardName, m_cardName);
		}
		GameLogic.Get().OutputGameLog(log);

		Player.Get().m_playerToken.transform.SetParent(gameObject.transform);

		var rt = Player.Get().m_playerToken.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.sizeDelta.x / 2, rt.sizeDelta.y / 2);
		rt.localScale = new Vector3(1, 1, 1);
	}
}
