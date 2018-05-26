using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGame : MonoBehaviour
{
	#region game UI
	public GameObject			m_actArea;
	public GameObject			m_agendaArea;
	public GameObject			m_selectCardInfo;
	public GameObject			m_gameArea;
	public List<GameObject>		m_playerCardArea;
	public Text					m_gameLog;
	public Button				m_InvestigateBtn;
	#endregion

	public ChaosBag				m_chaosBag;

	private GameObject			m_currentAct;
	private GameObject			m_currentAgenda;

	string[]	m_strScenarioPrefix = 
	{
		"cardprefabs/core_gathering_",
		"cardprefabs/core_mask_of_midnight_",
		"cardprefabs/core_devourer_below_",
	};

	string[]	m_roland_def_cards =
	{
		"Neutral/core_roland_dot38_special",
		"Neutral/core_cover_up",
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
		"Neutral/core_knife",
		"Neutral/core_knife",
		"Neutral/core_flashlight",
		"Neutral/core_flashlight",
		"Neutral/core_emergency_cache",
		"Neutral/core_emergency_cache",
		"Neutral/core_guts",
		"Neutral/core_guts",
		"Neutral/core_manual_dexterity",
		"Neutral/core_manual_dexterity",
		"Neutral/core_paranoia",
	};

	// Use this for initialization
	void Start ()
	{
		GameLogic.Get().m_logText = m_gameLog;

		// Load and instantiate prefabs
		string str = m_strScenarioPrefix[Player.Get().m_currentScenario];
		string strAct1 = str + "act_1";
		string strAgenda1 = str + "agenda_1";

		GameObject act1 = (GameObject)Resources.Load(strAct1);
		GameObject agenda1 = (GameObject)Resources.Load(strAgenda1);

		m_currentAct = Instantiate(act1);
		m_currentAgenda = Instantiate(agenda1);

		GameLogic.DockCard(Player.Get().m_investigatorCard.gameObject, GameObject.Find("InvestigatorCard"));
		GameLogic.DockCard(m_currentAct, m_actArea);
		GameLogic.DockCard(m_currentAgenda, m_agendaArea);

		_LoadPlayerCards(Player.Get().m_faction);
		_DrawFiveOpenHands();

		GameLogic.Get().StartScenario();

		string log = Player.Get().m_investigatorCard.m_cardName + "进入了场景。\n";
		GameLogic.Get().OutputGameLog(log);
	}

	private void _DrawFiveOpenHands()
	{
		for(int i=0; i<5; ++i)
		{
			Player.Get().AddHandCard(GameLogic.Get().DrawPlayerCard());
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

				GameLogic.Get().m_lstPlayerCards.Add(card);
			}
		}
		else
		{
			Debug.LogError("Error!!Not implement...");
		}

		GameLogic.Shuffle(GameLogic.Get().m_lstPlayerCards);
	}

	public void	OnButtonDrawPlayerCard()
	{
		Player.Get().AddHandCard(GameLogic.Get().DrawPlayerCard());
	}

	public void OnButtonInvestigateCurrentLocation()
	{
		m_gameArea.SetActive(false);
		m_selectCardInfo.SetActive(true);

		GameLogic.Get().m_cardClickMode = Card.CardClickMode.MultiSelect;

		GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "调查了" + Player.Get().m_currentLocation.m_cardName + "\n");
	}

	public void OnButtonConfirmSelectCard()
	{
		GameLogic.Get().InvestigateCurrentLocation(m_chaosBag);

		Card.m_lstSelectCards.ForEach(card => 
		{
			// Restore position
			RectTransform rt = (RectTransform)card.gameObject.GetComponent<RectTransform>().parent;
			rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y - 30);

			card.Discard();
		});

		m_gameArea.SetActive(true);
		m_selectCardInfo.SetActive(false);
		Card.m_lstSelectCards.Clear();
		GameLogic.Get().m_cardClickMode = Card.CardClickMode.Flip;
	}

	public void OnButtonGainOneResource()
	{
		Player.Get().m_resources += 1;
	}

	// Update is called once per frame
	void Update ()
	{
		// Display player hand cards
		var cards = Player.Get().GetHandCards();

		for(int i=0; i<cards.Count; ++i)
		{
			GameLogic.DockCard(cards[i].gameObject, m_playerCardArea[i]);
		}

		GameLogic.Get().Update();

		// Update UI status
		m_InvestigateBtn.GetComponent<Button>().interactable = Player.Get().m_currentLocation.m_clues > 0;
	}
}
