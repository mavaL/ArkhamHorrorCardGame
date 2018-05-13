using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class MainGame : MonoBehaviour
{
	public GameObject	m_actArea;
	public GameObject	m_agendaArea;

	private GameObject m_currentAct;
	private GameObject	m_currentAgenda;

	string[] m_strScenarioPrefix = 
	{
		"cardprefabs/core_gathering_",
		"cardprefabs/core_mask_of_midnight_",
		"cardprefabs/core_devourer_below_",
	};

	// Use this for initialization
	void Start ()
	{
		// Load and instantiate prefabs
		string str = m_strScenarioPrefix[GameLogic.Get().PlayerRole.m_currentScenario];
		string strAct1 = str + "act_1";
		string strAgenda1 = str + "agenda_1";

		GameObject act1 = (GameObject)Resources.Load(strAct1);
		GameObject agenda1 = (GameObject)Resources.Load(strAgenda1);

		m_currentAct = Instantiate(act1);
		m_currentAgenda = Instantiate(agenda1);

		m_currentAct.transform.SetParent(m_actArea.transform);
		m_currentAgenda.transform.SetParent(m_agendaArea.transform);

		m_currentAct.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		m_currentAct.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
		m_currentAgenda.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		m_currentAgenda.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
	}

	// Update is called once per frame
	void Update () {
		
	}
}
