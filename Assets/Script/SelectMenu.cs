using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SelectMenu : MonoBehaviour {

    public List<GameObject>		m_lstRoleInfo = new List<GameObject>();
    public List<GameObject>		m_lstScenarioInfo = new List<GameObject>();
	public List<GameObject>		m_lstPlayerToken = new List<GameObject>();

    public GameObject   m_selectRoleUI;
    public GameObject   m_selectScenarioUI;
    public Dropdown     m_selectRoleDropdown;
    public Dropdown     m_selectScenarioDropdown;

    // Use this for initialization
    void Start ()
    {
        // Load and instantiate prefabs
        _InitInvestigator("CardPrefabs/Investigator/core_invesigator_roland_banks", 0);
        _InitInvestigator("CardPrefabs/Investigator/core_invesigator_wendy_adams", 1);
        _InitInvestigator("CardPrefabs/Investigator/core_invesigator_agnes_baker", 2);
        _InitInvestigator("CardPrefabs/Investigator/core_invesigator_skids_otoole", 3);
        _InitInvestigator("CardPrefabs/Investigator/core_invesigator_daisy_walker", 4);

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

		GameLogic.DockCard(m_lstRoleInfo[index], GameObject.Find("RoleInfo"));
    }

    private void _InitScenario(string path, int index)
    {
        GameObject scenario = (GameObject)Resources.Load(path);

        m_lstScenarioInfo.Add(Instantiate(scenario));

		GameLogic.DockCard(m_lstScenarioInfo[index], GameObject.Find("ScenarioInfo"));
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
		GameLogic.Get().m_difficulty = (GameDifficulty)d.value;
    }

    public void OnButtonNextToSelectScenario()
    {
        m_selectRoleUI.SetActive(false);
        m_selectScenarioUI.SetActive(true);

        Player.Get().InitPlayer(m_lstRoleInfo[m_selectRoleDropdown.value].GetComponent<InvestigatorCard>());

		Player.Get().m_playerToken = Instantiate(m_lstPlayerToken[m_selectRoleDropdown.value]);
		Player.Get().m_playerToken.transform.SetParent(null);
		DontDestroyOnLoad(Player.Get().m_playerToken);

		_SetCurrentScenarioInfo(0);
    }

    public void OnButtonStartGame()
    {
		GameLogic.Get().InitChaosBag();

		Player.Get().m_faction = (Faction)m_selectRoleDropdown.value;
		Player.Get().m_currentScenario = m_selectScenarioDropdown.value;

		if(Player.Get().m_currentScenario == 0)
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("core_gathering");
		}
		else if (Player.Get().m_currentScenario == 1)
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("core_mask_of_midnight");
		}
		else if (Player.Get().m_currentScenario == 2)
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("core_devourer_below");
		}
	}
}
