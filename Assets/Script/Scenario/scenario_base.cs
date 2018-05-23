using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class scenario_base
{
	public GameObject		m_startLocation;
	public List<GameObject>	m_revealedLocations = new List<GameObject>();
	public List<GameObject>	m_lstOtherLocations;

	public void StartScenario()
	{
		GameLogic.DockCard(m_startLocation, GameObject.Find("StartLocation"));
		GameLogic.PlayerEnterLocation(m_startLocation);

		m_revealedLocations.Add(m_startLocation);
	}

	public abstract void ShowPlayInfo();
	public abstract int GetChaosTokenEffect(ChaosBag.ChaosTokenType t);
	public abstract void AfterSkillTestFailed(ChaosBag.ChaosTokenType t);
}
