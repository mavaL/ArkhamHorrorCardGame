/********************************************************************
	created:	2018/06/27
	created:	27:6:2018   0:38
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class core_medical_texts : PlayerCardLogic
{
	private PlayerAction m_cardAction;
	private UnityAction<int> m_onCardAction;
	private UnityAction m_onSkillTest;

	public override void OnReveal(Card card)
	{
		m_onCardAction = new UnityAction<int>(OnCardAction);
		m_onSkillTest = new UnityAction(OnButtonConfirmSkillTest);

		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_actionDropdown.options.Add(new Dropdown.OptionData("<医学文献>卡牌行动"));
		m_cardAction = (PlayerAction)ui.m_actionDropdown.options.Count - 1;
		ui.m_actionDropdown.onValueChanged.AddListener(m_onCardAction);
	}

	public override void OnDiscard(Card card)
	{
		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_actionDropdown.onValueChanged.RemoveListener(m_onCardAction);
		ui.m_actionDropdown.options.RemoveAt((int)m_cardAction);
	}

	private void OnCardAction(int index)
	{
		if (index == (int)m_cardAction)
		{
			GameLogic.Get().OutputGameLog(string.Format("{0}使用<医学文献>的卡牌行动\n", Player.Get().m_investigatorCard.m_cardName));

			bool bHurt = Player.Get().GetHp() < Player.Get().m_investigatorCard.m_health;
			UnityEngine.Assertions.Assert.IsTrue(bHurt, "Investigator is full HP in core_medical_texts.OnCardAction()!!!");

			var ui = GameLogic.Get().m_mainGameUI;
			ui.m_confirmChoiceBtn.gameObject.SetActive(true);
			ui.m_choiceMode = MainGame.ConfirmButtonMode.Custom;
			ui.m_confirmChoiceBtn.onClick.AddListener(m_onSkillTest);

			ui.BeginSelectCardToSpend();
		}
	}

	public void OnButtonConfirmSkillTest()
	{
		var ui = GameLogic.Get().m_mainGameUI;
		UnityEngine.Assertions.Assert.AreEqual(MainGame.ConfirmButtonMode.Custom, ui.m_choiceMode, "Assert failed in core_medical_texts.OnButtonConfirmSkillTest()!!");

		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = true;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = true;

		ui.m_confirmChoiceBtn.gameObject.SetActive(false);
		ui.m_confirmChoiceBtn.onClick.RemoveListener(m_onSkillTest);

		ChaosBag.ChaosTokenType chaosToken;
		int result = GameLogic.Get().SkillTest(SkillType.Intellect, 2, null, out chaosToken);
		bool bSucceed = result >= 0;
		bSucceed = true;

		if (bSucceed)
		{
			GameLogic.Get().OutputGameLog(string.Format("<医学文献>使用成功\n", Player.Get().m_investigatorCard.m_cardName));
			Player.Get().HealHealth(1);
		}
		else
		{
			GameLogic.Get().OutputGameLog(string.Format("<医学文献>使用失败\n", Player.Get().m_investigatorCard.m_cardName));
			Player.Get().DecreaseHealth(null, 1);
		}

		GameLogic.Get().AfterSkillTest(bSucceed, chaosToken);

		ui.EndSelectCardToSpend();

		Player.Get().ActionDone(m_cardAction);
	}

	private void Update()
	{
		GameLogic.Get().m_mainGameUI.m_isActionEnable[m_cardAction] = Player.Get().GetHp() < Player.Get().m_investigatorCard.m_health;
	}
}
