/********************************************************************
	created:	2018/07/02
	created:	2:7:2018   0:25
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_grasping_hands : TreacheryLogic
{
	private UnityAction<int, Card> m_afterSkillTest;

	public override void OnReveal()
	{
		var ui = GameLogic.Get().m_mainGameUI;
		var card = GetComponent<TreacheryCard>();
		ui.m_tempHighlightCard = card.gameObject;

		GameLogic.Get().ShowHighlightCardExclusive(card, false);
		ui.m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;
		ui.BeginSelectCardToSpend(SkillType.Agility);

		m_afterSkillTest = new UnityAction<int, Card>(AfterSkillTest);
		GameLogic.Get().m_afterSkillTest.AddListener(m_afterSkillTest);

		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = true;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = true;
	}

	public override void OnSkillTest()
	{
		GameLogic.Get().m_currentScenario.m_skillTest.AgilityTest(3, null);
	}

	public void AfterSkillTest(int result, Card Target)
	{
		if (result < 0)
		{
			int damage = -result;
			GameLogic.Get().OutputGameLog(string.Format("{0}由于<攥取之手>受到{1}点伤害！\n", Player.Get().m_investigatorCard.m_cardName, damage));

			Player.Get().DecreaseHealth(null, damage);

			GameLogic.Get().m_afterSkillTest.RemoveListener(m_afterSkillTest);
			GetComponent<TreacheryCard>().Discard();
		}
	}
}
