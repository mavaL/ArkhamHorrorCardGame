using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler {

    public Sprite   m_frontImage;
    public Sprite   m_backImage;
    public Image    m_image = null;
    public string   m_cardName;
    public bool     m_canFocus = false;

	public int		m_cluesInLocation = 0;
	public int		m_shroud = 0;

    private bool    m_bIsFront = true;
    private bool    m_bIsFocus = false;
    private GameObject  m_focusImage;

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
        if(m_bIsFront)
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
