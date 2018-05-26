using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationCard : Card
{
	public int					m_clues = 0;
	public int					m_shroud = 0;
	public List<EnemyCard>		m_lstEnemies = new List<EnemyCard>();
}
