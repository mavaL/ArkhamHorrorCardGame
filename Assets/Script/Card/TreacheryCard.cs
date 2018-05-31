using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



[System.Serializable]
public class SkillTestEvent : UnityEvent<int> { }

public class TreacheryCard : Card
{
	public SkillTestEvent	m_skillTestEvent;

	public override void Discard()
	{
		gameObject.transform.SetParent(GameObject.Find("Canvas").transform);
		gameObject.SetActive(false);
		GameLogic.Get().m_lstDiscardEncounterCards.Add(gameObject);
	}
}
