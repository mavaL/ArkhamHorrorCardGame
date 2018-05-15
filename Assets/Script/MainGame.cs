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

	string[]	m_strScenarioPrefix = 
	{
		"cardprefabs/core_gathering_",
		"cardprefabs/core_mask_of_midnight_",
		"cardprefabs/core_devourer_below_",
	};

	string[]	m_roland_def_cards =
	{
		"core_roland_dot38_special",
		"core_cover_up",
		"Guardian/core_guardian_dot45_automatic",
		"Guardian/core_guardian_physical_training",
		"Guardian/core_guardian_beat_cop",
		"Guardian/core_guardian_first_aid",
		"Guardian/core_guardian_machete",
		"Guardian/core_guardian_dog",
		"Guardian/core_guardian_evidence",
		"Guardian/core_guardian_dodge",
		"Guardian/core_guardian_dynamite_blast",
		"Guardian/core_guardian_vicious_blow",
		"Seeker/core_seeker_magnifying_glass",
		"Seeker/core_seeker_old_book_of_lore",
		"Seeker/core_seeker_research_librarian",
		"Seeker/core_seeker_milan_christopher",
		"Seeker/core_seeker_hyperawreness",
		"Seeker/core_seeker_medical_texts",
		"Seeker/core_seeker_mind_over_matter",
		"Seeker/core_seeker_working_a_hunch",
		"Seeker/core_seeker_barricade",
		"Seeker/core_seeker_deduction",
		// TODO: 重复加载资源？
		"core_knife",
		"core_knife",
		"core_flashlight",
		"core_flashlight",
		"core_emergency_cache",
		"core_emergency_cache",
		"core_guts",
		"core_guts",
		"core_manual_dexterity",
		"core_manual_dexterity",
		"core_paranoia",
	};

	private List<GameObject>		m_lstPlayerCards = new List<GameObject>();
	private List<GameObject>		m_lstEncounterCards = new List<GameObject>();

	// Use this for initialization
	void Start ()
	{
		// Load and instantiate prefabs
		string str = m_strScenarioPrefix[GameLogic.Get().m_player.m_currentScenario];
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

		_LoadPlayerCards(GameLogic.Get().m_player.m_faction);
		_DrawFiveOpenHands();
	}

	private void _DrawFiveOpenHands()
	{
		for(int i=0; i<5; ++i)
		{
			GameLogic.Get().m_player.AddHandCard(GameLogic.DrawCard(m_lstPlayerCards));
		}
	}

	private void _LoadPlayerCards(Faction faction)
	{
		// Currently using default card set
		if(faction == Faction.Guardian)
		{
			for(int i=0; i<m_roland_def_cards.Length; ++i)
			{
				string path = "CardPrefabs/PlayerCards/" + m_roland_def_cards[i];
				GameObject card = Instantiate((GameObject)Resources.Load(path));
				card.SetActive(false);

				m_lstPlayerCards.Add(card);
			}
		}
		else
		{
			Debug.LogError("Error!!Not implement...");
		}

		GameLogic.Shuffle(m_lstPlayerCards);
	}

	public void	OnButtonDrawPlayerCard()
	{

	}

	public void OnButtonDrawEncounterCard()
	{

	}

	// Update is called once per frame
	void Update () {
		
	}
}
