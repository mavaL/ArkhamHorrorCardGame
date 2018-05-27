using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationCard : Card
{
	public int					m_clues = 0;
	public int					m_shroud = 0;
	[System.NonSerialized]
	public List<EnemyCard>		m_lstEnemies = new List<EnemyCard>();

	public void EnterLocation()
	{
		m_canFlip = true;
		FlipCard();
		Player.Get().m_playerToken.transform.SetParent(gameObject.transform);

		var rt = Player.Get().m_playerToken.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.sizeDelta.x / 2, rt.sizeDelta.y / 2);
		rt.localScale = new Vector3(1, 1, 1);
	}
}
