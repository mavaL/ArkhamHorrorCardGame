using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
	void Awake()
	{
		if (m_image == null)
		{
			m_image = gameObject.AddComponent<Image>();
			m_image.sprite = m_frontImage;
		}
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
		Fast
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

	private bool    m_bIsFront = true;
    private bool    m_bIsFocus = false;
	private bool	m_bSelected = false;
    private GameObject			m_focusImage;
	public static List<Card>	m_lstSelectCards = new List<Card>();
	public bool					m_exhausted { set; get; } = false;
	public LocationCard			m_currentLocation { set; get; }


	// Use this for initialization
	void Start ()
    {
		if(m_canFocus)
		{
			var trigger = gameObject.AddComponent<EventTrigger>();
			{

				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerEnter;
				entry.callback = new EventTrigger.TriggerEvent();
				UnityAction<BaseEventData> callback = new UnityAction<BaseEventData>(OnPointerEnter);
				entry.callback.AddListener(callback);
				trigger.triggers.Add(entry);
			}

			{
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerExit;
				entry.callback = new EventTrigger.TriggerEvent();
				UnityAction<BaseEventData> callback = new UnityAction<BaseEventData>(OnPointerExit);
				entry.callback.AddListener(callback);
				trigger.triggers.Add(entry);
			}
		}
	}

	public void FlipCard()
	{
		if (m_bIsFront)
		{
			m_image.sprite = m_backImage;
			m_bIsFront = false;
		}
		else
		{
			m_image.sprite = m_frontImage;
			m_bIsFront = true;
		}
	}

    public void OnPointerClick(PointerEventData eventData)
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
					RectTransform rt = (RectTransform)gameObject.GetComponent<RectTransform>().parent;
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y - 30);

					m_lstSelectCards.Remove(this);
					m_bSelected = false;
				}
				else
				{
					RectTransform rt = (RectTransform)gameObject.GetComponent<RectTransform>().parent;
					rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + 30);

					m_lstSelectCards.Add(this);
					m_bSelected = true;
				}
			}
		}
    }

    public void OnPointerEnter(BaseEventData arg0)
    {
        if(m_canFocus)
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
        if(m_bIsFocus)
        {
			m_image.raycastTarget = true;
			GameObject.Destroy(m_focusImage);
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
