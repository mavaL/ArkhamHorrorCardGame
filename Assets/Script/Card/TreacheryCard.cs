using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



[System.Serializable]
public class SkillTestEvent : UnityEvent<int> { }

public class TreacheryCard : Card
{
	public bool m_persistent = false;

	public override void Discard()
	{
		base.Discard();

		if(m_persistent)
		{
			Player.Get().RemoveTreachery(this);
		}

		gameObject.transform.SetParent(GameLogic.Get().m_mainGameUI.transform.root.parent);
		gameObject.SetActive(false);
		GameLogic.Get().m_lstDiscardEncounterCards.Add(gameObject);
	}

	public override void OnSkillTest()
	{
		GetComponent<TreacheryLogic>().OnSkillTest();
	}
}
