using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCard : Card
{
	public enum Keyword
	{
		Ghoul
	}

	public List<Keyword>	m_lstKeywords = new List<Keyword>();

	public bool IsKeywordContain(Keyword k)
	{
		return m_lstKeywords.Contains(k);
	}
}
