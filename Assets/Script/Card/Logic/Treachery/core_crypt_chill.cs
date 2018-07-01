/********************************************************************
	created:	2018/06/29
	created:	29:6:2018   8:36
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_crypt_chill : TreacheryLogic
{
	private UnityAction<int, Card> m_afterSkillTest;

	public override void OnReveal()
	{
		m_afterSkillTest = new UnityAction<int, Card>(AfterSkillTest);

		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;
		ui.BeginSelectCardToSpend(SkillType.Willpower);
		GameLogic.Get().m_afterSkillTest.AddListener(m_afterSkillTest);

		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = true;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = true;
	}

	public override void OnSkillTest()
	{
		GameLogic.Get().m_currentScenario.m_skillTest.WillpowerTest(4, null);
	}

	public void AfterSkillTest(int result, Card Target)
	{
		if (result < 0)
		{
			if (!Player.Get().ChooseAndDiscardAssetCard())
			{
				Player.Get().DecreaseHealth(null, 2);
				GameLogic.Get().OutputGameLog(string.Format("{0}结算<地穴恶寒>，因为没有在场资产牌，受到了2点伤害！\n", Player.Get().m_investigatorCard.m_cardName));
			}
		}

		GetComponent<TreacheryCard>().Discard();
	}
}
