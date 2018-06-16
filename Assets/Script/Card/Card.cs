using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
	void Awake()
	{
		if (m_image == null)
		{
			m_image = gameObject.AddComponent<Image>();
			m_image.sprite = m_frontImage;
		}
		m_thisInListView = this;
	}

	public enum CardClickMode
	{
		Flip,
		MultiSelect
	}

	public enum Keyword
	{
		Ghoul,
		Item,
		Tome,
		Spell,
		Hunter,
		Hazard,
		Elite,
		Retaliate,
		Prey,	// TODO: multi-players need this
		Asset,
		Event,
		Skill,
		Fast,
		Ally
	}

	public List<Keyword> m_lstKeywords = new List<Keyword>();

	public bool IsKeywordContain(Keyword k)
	{
		return m_lstKeywords.Contains(k);
	}

	public static int HowManyEnemyCardContainTheKeyword(List<Card> cards, Keyword k)
	{
		int num = 0;
		for (int i = 0; i < cards.Count; ++i)
		{
			if (cards[i] is EnemyCard && cards[i].m_lstKeywords.Contains(k))
			{
				++num;
			}
		}
		return num;
	}

	public static int HowManyPlayerCardContainTheKeyword(List<PlayerCard> cards, Keyword k)
	{
		int num = 0;
		for (int i = 0; i < cards.Count; ++i)
		{
			if (cards[i].m_lstKeywords.Contains(k))
			{
				++num;
			}
		}
		return num;
	}

	public Sprite   m_frontImage;
    public Sprite   m_backImage;
    public Image    m_image = null;
    public string   m_cardName;
    public bool     m_canFocus = false;
	public bool		m_canFlip = true;
	public bool		m_autoEventTrigger = true;

	private bool    m_bIsFront = true;
    private bool    m_bIsFocus = false;
	private bool	m_bSelected = false;
	private EventTrigger		m_eventTrigger;
	private GameObject			m_focusImage;
	public static List<Card>	m_lstSelectCards = new List<Card>();
	public bool					m_exhausted { set; get; } = false;
	public Card					m_thisInListView { set; get; }
	public LocationCard			m_currentLocation { set; get; }


	// Use this for initialization
	void Start ()
    {
		if(m_canFocus && m_autoEventTrigger)
		{
			BindEventTrigger(EventTriggerType.PointerEnter, new UnityAction<BaseEventData>(OnPointerEnter));
			BindEventTrigger(EventTriggerType.PointerExit, new UnityAction<BaseEventData>(OnPointerExit));
			BindEventTrigger(EventTriggerType.PointerClick, new UnityAction<BaseEventData>(OnPointerClick));
		}
	}

	public void BindEventTrigger(EventTriggerType type, UnityAction<BaseEventData> func)
	{
		if(m_eventTrigger == null)
		{
			m_eventTrigger = gameObject.AddComponent<EventTrigger>();
		}
		else
		{
			for(int i=0; i< m_eventTrigger.triggers.Count; ++i)
			{
				if(m_eventTrigger.triggers[i].eventID == type)
				{
					// Already binded, replace it
					m_eventTrigger.triggers.RemoveAt(i);
					break;
				}
			}
		}

		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = type;
		entry.callback = new EventTrigger.TriggerEvent();
		entry.callback.AddListener(func);
		m_eventTrigger.triggers.Add(entry);
	}

	public void FlipCard()
	{
		if (m_bIsFront)
		{
			m_thisInListView.m_image.sprite = m_backImage;
			m_image.sprite = m_backImage;
			if(m_focusImage)
			{
				m_focusImage.GetComponent<Image>().sprite = m_backImage;
			}
			m_bIsFront = false;
		}
		else
		{
			m_thisInListView.m_image.sprite = m_frontImage;
			m_image.sprite = m_frontImage;
			if (m_focusImage)
			{
				m_focusImage.GetComponent<Image>().sprite = m_frontImage;
			}
			m_bIsFront = true;
		}
	}

    public void OnPointerClick(BaseEventData eventData)
    {
		if(m_canFlip && GameLogic.Get().m_cardClickMode == CardClickMode.Flip)
		{
			FlipCard();
		}
        else if(GameLogic.Get().m_cardClickMode == CardClickMode.MultiSelect)
		{
			PlayerCard pc = this as PlayerCard;

			if(pc != null && pc.m_skillIcons.Count > 0)
			{
				if (m_bSelected)
				{
					RectTransform rt = (RectTransform)m_thisInListView.gameObject.GetComponent<RectTransform>().parent;
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y - 30);

					m_lstSelectCards.Remove(this);
					m_bSelected = false;
				}
				else
				{
					RectTransform rt = (RectTransform)m_thisInListView.gameObject.GetComponent<RectTransform>().parent;
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + 30);

					m_lstSelectCards.Add(this);
					m_bSelected = true;
				}
			}
		}
    }

    public void OnPointerEnter(BaseEventData arg0)
    {
        if(m_canFocus && !m_focusImage)
        {
			gameObject.SetActive(true);
            m_focusImage = GameObject.Instantiate(gameObject);
			m_focusImage.GetComponent<Image>().raycastTarget = false;

			GameLogic.DockCard(m_focusImage, GameObject.Find("Canvas"), 0, false);

            m_bIsFocus = true;
        }
    }

    public void OnPointerExit(BaseEventData arg0)
    {
        if(m_bIsFocus && m_focusImage)
        {
			m_image.raycastTarget = true;
			GameObject.Destroy(m_focusImage);
			m_focusImage = null;
			m_bIsFocus = false;
        }
    }

	public virtual void Discard() {}
	public virtual void OnSkillTest() {}
	public virtual void OnSkillTestResult(int result) {}
	public virtual void OnSpawnAtLocation(LocationCard loc) { }
	public virtual void OnRecoverFromExhaust() { }
	public virtual void OnExhausted() { }
}
