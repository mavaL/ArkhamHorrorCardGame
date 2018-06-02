using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



[System.Serializable]
public class SkillTestEvent : UnityEvent<int> { }

public class TreacheryCard : Card
{
	public SkillTestEvent	m_skillTestEvent;
	public SkillTestEvent	m_skillTestResultEvent;

	public override void Discard()
	{
		gameObject.transform.SetParent(GameObject.Find("Canvas").transform);
		gameObject.SetActive(false);
		GameLogic.Get().m_lstDiscardEncounterCards.Add(gameObject);
	}

	public override void OnSkillTest()
	{
		m_skillTestEvent.Invoke(0);
		Discard();
	}

	public override void OnSkillTestResult(int result)
	{
		if (result < 0)
		{
			m_skillTestResultEvent.Invoke(-result);
		}
	}
}
