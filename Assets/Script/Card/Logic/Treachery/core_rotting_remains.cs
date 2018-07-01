/********************************************************************
	created:	2018/07/01
	created:	1:7:2018   23:10
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_rotting_remains : TreacheryLogic
{
	private UnityAction<int, Card> m_afterSkillTest;

	public override void OnReveal()
	{
		var ui = GameLogic.Get().m_mainGameUI;
		var card = GetComponent<TreacheryCard>();
		ui.m_tempHighlightCard = card.gameObject;

		GameLogic.Get().ShowHighlightCardExclusive(card, false);
		ui.m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;
		ui.BeginSelectCardToSpend(SkillType.Willpower);

		m_afterSkillTest = new UnityAction<int, Card>(AfterSkillTest);
		GameLogic.Get().m_afterSkillTest.AddListener(m_afterSkillTest);

		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = true;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = true;
	}

	public override void OnSkillTest()
	{
		GameLogic.Get().m_currentScenario.m_skillTest.WillpowerTest(3, null);
	}

	public void AfterSkillTest(int result, Card Target)
	{
		if (result < 0)
		{
			int horror = -result;
			GameLogic.Get().OutputGameLog(string.Format("{0}由于<腐烂遗骸>受到{1}点恐怖！\n", Player.Get().m_investigatorCard.m_cardName, horror));

			Player.Get().DecreaseSanity(horror);

			GameLogic.Get().m_afterSkillTest.RemoveListener(m_afterSkillTest);
			GetComponent<TreacheryCard>().Discard();
		}
	}
}
