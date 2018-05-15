﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SelectMenu : MonoBehaviour {

    public List<GameObject>   m_lstRoleInfo = new List<GameObject>();
    public List<GameObject>   m_lstScenarioInfo = new List<GameObject>();

    public GameObject   m_selectRoleUI;
    public GameObject   m_selectScenarioUI;
    public Dropdown     m_selectRoleDropdown;
    public Dropdown     m_selectScenarioDropdown;

    // Use this for initialization
    void Start ()
    {
        // Load and instantiate prefabs
        _InitInvestigator("CardPrefabs/core_invesigator_roland_banks", 0);
        _InitInvestigator("CardPrefabs/core_invesigator_wendy_adams", 1);
        _InitInvestigator("CardPrefabs/core_invesigator_agnes_baker", 2);
        _InitInvestigator("CardPrefabs/core_invesigator_skids_otoole", 3);
        _InitInvestigator("CardPrefabs/core_invesigator_daisy_walker", 4);

        _InitScenario("CardPrefabs/core_gathering", 0);
        _InitScenario("CardPrefabs/core_mask_of_midnight", 1);
        _InitScenario("CardPrefabs/core_devourer_below", 2);

        _SetCurrentRoleInfo(0);

        // Init dropdown control
        List<string> roleNames = new List<string>();
        for(int i=0; i<m_lstRoleInfo.Count; ++i)
        {
            Card card = m_lstRoleInfo[i].GetComponent<Card>();
            roleNames.Add(card.m_cardName);
        }
        m_selectRoleDropdown.AddOptions(roleNames);

        List<string> scenarioNames = new List<string>();
        for (int i = 0; i < m_lstScenarioInfo.Count; ++i)
        {
            Card card = m_lstScenarioInfo[i].GetComponent<Card>();
            scenarioNames.Add(card.m_cardName);
        }
        m_selectScenarioDropdown.AddOptions(scenarioNames);

        m_selectRoleUI.SetActive(true);
        m_selectScenarioUI.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void _InitInvestigator(string path, int index)
    {
        GameObject investigator = (GameObject)Resources.Load(path);

        m_lstRoleInfo.Add(Instantiate(investigator));

        m_lstRoleInfo[index].transform.SetParent(GameObject.Find("RoleInfo").transform);
        m_lstRoleInfo[index].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }

    private void _InitScenario(string path, int index)
    {
        GameObject scenario = (GameObject)Resources.Load(path);

        m_lstScenarioInfo.Add(Instantiate(scenario));

        m_lstScenarioInfo[index].transform.SetParent(GameObject.Find("ScenarioInfo").transform);
        m_lstScenarioInfo[index].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        m_lstScenarioInfo[index].GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
    }

    private void _SetCurrentRoleInfo(int index)
    {
        for(int i=0; i<m_lstRoleInfo.Count; ++i)
        {
            if(i == index)
            {
                m_lstRoleInfo[i].SetActive(true);
            }
            else
            {
                m_lstRoleInfo[i].SetActive(false);
            }
        }
    }

    private void _SetCurrentScenarioInfo(int index)
    {
        for (int i = 0; i < m_lstScenarioInfo.Count; ++i)
        {
            if (i == index)
            {
                m_lstScenarioInfo[i].SetActive(true);
            }
            else
            {
                m_lstScenarioInfo[i].SetActive(false);
            }
        }
    }

    public void OnSelectRoleChanged(Dropdown d)
    {
        _SetCurrentRoleInfo(d.value);
    }

    public void OnSelectScenarioChanged(Dropdown d)
    {
        _SetCurrentScenarioInfo(d.value);
    }

    public void OnSelectDifficultyChanged(Dropdown d)
    {

    }

    public void OnButtonNextToSelectScenario()
    {
        m_selectRoleUI.SetActive(false);
        m_selectScenarioUI.SetActive(true);

        _SetCurrentScenarioInfo(0);
    }

    public void OnButtonStartGame()
    {
		GameLogic.Get().m_player.m_faction = (Faction)m_selectRoleDropdown.value;
		GameLogic.Get().m_player.m_currentScenario = m_selectScenarioDropdown.value;

		UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
    }
}
