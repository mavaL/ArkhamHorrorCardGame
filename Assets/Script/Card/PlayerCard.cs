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

[System.Serializable]
 public class SkillIconDictionary : SerializableDictionary<SkillType, int> {}


public class PlayerCard : Card
{
	public SkillIconDictionary	m_skillIcons;
	public bool					m_isPlayerDeck = true;

	public override void Discard()
	{
		gameObject.transform.SetParent(GameObject.Find("Canvas").transform);
		gameObject.SetActive(false);
		Player.Get().DiscardHandCard(this);
		GameLogic.Get().m_lstDiscardPlayerCards.Add(gameObject);
	}
}
