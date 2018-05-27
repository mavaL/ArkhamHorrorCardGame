using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class scenario_base : MonoBehaviour
{
	public GameObject		m_startLocation;
	public Text				m_playerInfoText;
	[System.NonSerialized]
	public List<GameObject>	m_revealedLocations = new List<GameObject>();

	public List<GameObject> m_lstActCards;
	public List<GameObject> m_lstAgendaCards;

	[System.NonSerialized]
	public QuestCard		m_currentAct;
	[System.NonSerialized]
	public QuestCard		m_currentAgenda;
	[System.NonSerialized]
	public int				m_indexCurrentAct;
	[System.NonSerialized]
	public int				m_indexCurrentAgenda;

	public abstract void ShowPlayInfo();
	public abstract int GetChaosTokenEffect(ChaosBag.ChaosTokenType t);
	public abstract void AfterSkillTestFailed(ChaosBag.ChaosTokenType t);
	public abstract void AdvanceAct();
}
