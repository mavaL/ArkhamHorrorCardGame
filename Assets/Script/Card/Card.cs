﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
	public enum CardClickMode
	{
		Flip,
		MultiSelect
	}

    public Sprite   m_frontImage;
    public Sprite   m_backImage;
    public Image    m_image = null;
    public string   m_cardName;
    public bool     m_canFocus = false;

    private bool    m_bIsFront = true;
    private bool    m_bIsFocus = false;
	private bool	m_bSelected = false;
    private GameObject			m_focusImage;
	public static List<Card>	m_lstSelectCards = new List<Card>();


	// Use this for initialization
	void Start ()
    {
        if(m_image == null)
        {
            m_image = this.gameObject.AddComponent<Image>();
            m_image.sprite = m_frontImage;
        }

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
	
	// Update is called once per frame
	void Update ()
	{
		
	}

    public void OnPointerClick(PointerEventData eventData)
    {
		if(GameLogic.Get().m_cardClickMode == CardClickMode.Flip)
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
        else if(GameLogic.Get().m_cardClickMode == CardClickMode.MultiSelect)
		{
			if(m_bSelected)
			{ 
				RectTransform rt = (RectTransform)gameObject.GetComponent<RectTransform>().parent;
				rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y-30);

				m_lstSelectCards.Remove(this);
				m_bSelected = false;
			}
			else
			{
				RectTransform rt = (RectTransform)gameObject.GetComponent<RectTransform>().parent;
				rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y+30);

				m_lstSelectCards.Add(this);
				m_bSelected = true;
			}
		}
    }

    public void OnPointerEnter(BaseEventData arg0)
    {
        if(m_canFocus)
        {
            m_focusImage = GameObject.Instantiate(gameObject);
			m_focusImage.GetComponent<Image>().raycastTarget = false;

            RectTransform rt = (RectTransform)m_image.GetComponent<RectTransform>().parent;
            m_focusImage.GetComponent<RectTransform>().SetParent(GameObject.Find("Canvas").transform);
            m_focusImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            m_focusImage.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);

            m_bIsFocus = true;
        }
    }

    public void OnPointerExit(BaseEventData arg0)
    {
        if(m_bIsFocus)
        {
            GameObject.Destroy(m_focusImage);
            m_bIsFocus = false;
        }
    }
}
