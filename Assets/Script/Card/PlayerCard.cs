using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

 [System.Serializable]
 public class SkillIconDictionary : SerializableDictionary<PlayerCard.SkillIcon, int> {}


public class PlayerCard : Card
{
	public enum SkillIcon
	{
		Willpower,
		Intellect,
		Combat,
		Agility,
		Wild	// Can be treat as any other
	}

	public SkillIconDictionary m_skillIcons;

	public override void Discard()
	{
		gameObject.transform.SetParent(GameObject.Find("Canvas").transform);
		gameObject.SetActive(false);
		Player.Get().DiscardHandCard(this);
		GameLogic.Get().m_lstDiscardPlayerCards.Add(gameObject);
	}
}
