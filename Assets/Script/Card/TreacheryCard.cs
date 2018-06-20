using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



[System.Serializable]
public class SkillTestEvent : UnityEvent<int> { }

public class TreacheryCard : Card
{
	public bool				m_discardAfterReveal = true;
	public SkillTestEvent	m_skillTestEvent;
	public SkillTestEvent	m_skillTestResultEvent;
	public UnityEvent		m_onRevealEvent = new UnityEvent();

	public override void Discard(bool bFromAssetArea = false)
	{
		base.Discard(bFromAssetArea);

		gameObject.transform.SetParent(GameLogic.Get().m_mainGameUI.transform.root.parent);
		gameObject.SetActive(false);
		GameLogic.Get().m_lstDiscardEncounterCards.Add(gameObject);
	}

	public override void OnSkillTest()
	{
		m_skillTestEvent.Invoke(0);

		if(m_discardAfterReveal)
		{
			Discard();
		}
	}

	public override void OnSkillTestResult(int result)
	{
		m_skillTestResultEvent.Invoke(result);
	}
}
