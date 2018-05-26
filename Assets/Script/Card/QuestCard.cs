using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestCard : Card
{
	public int	m_tokenToAdvance;
	[System.NonSerialized]
	public int	m_currentToken = 0;
}
