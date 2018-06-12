using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Must be sync with EncounterEvents.SkillTest() parameter!
public enum SkillType
{
	Willpower,
	Intellect,
	Combat,
	Agility,
	Wild    // Can be treat as any other
}

public enum EventTiming
{
	None,
	EnemyAttack
}

[System.Serializable]
 public class SkillIconDictionary : SerializableDictionary<SkillType, int> {}


public class PlayerCard : Card
{
	public SkillIconDictionary	m_skillIcons;
	public bool					m_isPlayerDeck = true;
	public EventTiming			m_eventTiming = EventTiming.None;

	public override void Discard()
	{
		gameObject.transform.SetParent(GameObject.Find("Canvas").transform);
		gameObject.SetActive(false);
		Player.Get().DiscardHandCard(this);
		GameLogic.Get().m_lstDiscardPlayerCards.Add(gameObject);
	}

	public void PlayIt()
	{
		if(m_lstKeywords.Contains(Keyword.Asset))
		{

		}
		else
		{
			UnityEngine.Assertions.Assert.IsTrue(m_lstKeywords.Contains(Keyword.Event), "Assert failed in PlayerCard.PlayIt()!!!");

			GetComponent<PlayerCardLogic>().OnReveal(this);
		}

		Player.Get().ActionDone(PlayerAction.PlayCard, !m_lstKeywords.Contains(Keyword.Fast));
	}
}
