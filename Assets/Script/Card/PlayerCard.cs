/********************************************************************
	created:	2018/06/16
	created:	16:6:2018   19:35
	author:		maval
	
	TODO:	    1. Asset cards should obey slot limitation.
*********************************************************************/

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
	EnemyAttack,
	DefeatEnemy,
	InvestigatePhase,
	BeforeSkillTest
}

[System.Serializable]
public class SkillIconDictionary : SerializableDictionary<SkillType, int> {}
[System.Serializable]
public class SkillCardEffect : UnityEvent<int, GameObject> { }

public class PlayerCard : Card
{
	public SkillIconDictionary	m_skillIcons;
	public int					m_cost;
	public AssetSlot			m_slot = AssetSlot.None;
	public bool					m_isPlayerDeck = true;
	public EventTiming			m_eventTiming = EventTiming.None;
	public SkillCardEffect		m_skillCardEffect;

	public override void Discard(bool bFromAssetArea = false)
	{
		base.Discard(bFromAssetArea);

		if (GetComponent<PlayerCardLogic>() && bFromAssetArea)
		{
			GetComponent<PlayerCardLogic>().OnDiscard(this);
		}

		gameObject.transform.SetParent(GameObject.Find("Canvas").transform);
		gameObject.SetActive(false);
		Player.Get().RemoveHandCard(this);
		GameLogic.Get().m_lstDiscardPlayerCards.Add(gameObject);
	}

	public IEnumerator PlayIt()
	{
		Player.Get().m_resources -= m_cost;

		if (m_lstKeywords.Contains(Keyword.Asset))
		{
			Player.Get().AddAssetCard(this);
		}
		else
		{
			UnityEngine.Assertions.Assert.IsTrue(m_lstKeywords.Contains(Keyword.Event), "Assert failed in PlayerCard.PlayIt()!!!");

			GetComponent<PlayerCardLogic>().OnReveal(this);

			yield return new WaitUntil(() => Player.Get().m_currentAction.Count == 0);

			Discard();
		}

		Player.Get().ActionDone(PlayerAction.PlayCard, !m_lstKeywords.Contains(Keyword.Fast));
	}
}
