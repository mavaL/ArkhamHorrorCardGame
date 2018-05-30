using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestCard : Card
{
	public int	m_tokenToAdvance;
	[System.NonSerialized]
	public int	m_currentToken = 0;

	public bool AddDoom(int num = 1)
	{
		UnityEngine.Assertions.Assert.IsTrue(m_currentToken < m_tokenToAdvance, "Assertion failed in AddDoom()!!");
		m_currentToken += num;

		GameLogic.Get().OutputGameLog(string.Format("恶兆增加了<color=red>{0}</color>\n", num));

		return m_currentToken >= m_tokenToAdvance;
	}
}
