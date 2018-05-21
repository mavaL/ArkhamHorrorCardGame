using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationCard : Card
{
	public int					m_clues = 0;
	public int					m_shroud = 0;
	public List<EnemyCard>		m_lstEnemies = new List<EnemyCard>();

	public int HowManyEnemyContainTheKeyword(EnemyCard.Keyword k)
	{
		int num = 0;
		for(int i=0; i<m_lstEnemies.Count; ++i)
		{
			if(m_lstEnemies[i].m_lstKeywords.Contains(k))
			{
				++num;
			}
		}
		return num;
	}
}
