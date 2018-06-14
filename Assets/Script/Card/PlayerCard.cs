﻿using System.Collections;
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
	EnemyAttack,
	DefeatEnemy,
	InvestigatePhase
}

[System.Serializable]
public class SkillIconDictionary : SerializableDictionary<SkillType, int> {}
[System.Serializable]
public class SkillCardEffect : UnityEvent<int, GameObject> { }

public class PlayerCard : Card
{
	public SkillIconDictionary	m_skillIcons;
	public int					m_cost;
	public bool					m_isPlayerDeck = true;
	public EventTiming			m_eventTiming = EventTiming.None;
	public SkillCardEffect		m_skillCardEffect;

	public override void Discard()
	{
		gameObject.transform.SetParent(GameObject.Find("Canvas").transform);
		gameObject.SetActive(false);
		Player.Get().DiscardHandCard(this);
		GameLogic.Get().m_lstDiscardPlayerCards.Add(gameObject);
	}

	public IEnumerator PlayIt()
	{
		if(m_lstKeywords.Contains(Keyword.Asset))
		{

		}
		else
		{
			UnityEngine.Assertions.Assert.IsTrue(m_lstKeywords.Contains(Keyword.Event), "Assert failed in PlayerCard.PlayIt()!!!");

			GetComponent<PlayerCardLogic>().OnReveal(this);
		}

		Player.Get().m_resources -= m_cost;

		yield return new WaitUntil(()=> Player.Get().m_currentAction == PlayerAction.None);

		Discard();

		Player.Get().ActionDone(PlayerAction.PlayCard, !m_lstKeywords.Contains(Keyword.Fast));
	}
}
