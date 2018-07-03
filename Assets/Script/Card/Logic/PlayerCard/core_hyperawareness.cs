﻿/********************************************************************
	created:	2018/06/26
	created:	26:6:2018   23:40
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_hyperawareness : PlayerCardLogic
{
	private UnityAction<int, Card> m_afterSkillTest;
	private bool	m_option1or2;

	public override void OnReveal(Card card)
	{
		m_afterSkillTest = new UnityAction<int, Card>(AfterSkillTest);
		Player.Get().m_currentAction.Pop();
	}

	public override bool CanTrigger()
	{
		return Player.Get().m_resources > 0;
	}

	public override void OnUseReactiveAsset()
	{
		List<string> options = new List<string>();
		options.Add("花费1资源，本次检定+1知识");
		options.Add("花费1资源，本次检定+1敏捷");

		var mainUI = GameLogic.Get().m_mainGameUI;
		mainUI.m_choiceDropdown.ClearOptions();
		mainUI.m_choiceDropdown.AddOptions(options);
		mainUI.m_choiceDropdown.RefreshShownValue();

		mainUI.m_choiceDropdown.gameObject.SetActive(true);
		mainUI.m_confirmChoiceBtn.gameObject.SetActive(true);
		mainUI.m_choiceMode = MainGame.ConfirmButtonMode.TextOnly;

		mainUI.m_lstChoiceEvent.Clear();
		mainUI.m_lstChoiceEvent.Add(new ChoiceEvent());
		mainUI.m_lstChoiceEvent.Add(new ChoiceEvent());

		mainUI.m_lstChoiceEvent[0].AddListener(new UnityAction<object>(OnOption1));
		mainUI.m_lstChoiceEvent[1].AddListener(new UnityAction<object>(OnOption2));
	}

	private void OnOption1(object param)
	{
		m_option1or2 = true;
		_OnChoosedOption();
	}

	private void OnOption2(object param)
	{
		m_option1or2 = false;
		_OnChoosedOption();
	}

	private void _OnChoosedOption()
	{
		UnityEngine.Assertions.Assert.IsTrue(Player.Get().m_resources > 0, "Assert failed in core_hyperawareness._OnChoosedOption()!!!");


		if (m_option1or2)
		{
			Player.Get().m_investigatorCard.m_intellect += 1;
		}
		else
		{
			Player.Get().m_investigatorCard.m_agility += 1;
		}
		Player.Get().m_resources -= 1;

		GameLogic.Get().m_afterSkillTest.AddListener(m_afterSkillTest);

		var mainUI = GameLogic.Get().m_mainGameUI;
		mainUI.m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;

		GameLogic.Get().m_currentTiming = EventTiming.None;
		Player.Get().m_currentAction.Pop();
	}

	private void AfterSkillTest(int result, Card target)
	{
		GameLogic.Get().m_afterSkillTest.RemoveListener(m_afterSkillTest);

		if (m_option1or2)
		{
			Player.Get().m_investigatorCard.m_intellect -= 1;
		}
		else
		{
			Player.Get().m_investigatorCard.m_agility -= 1;
		}
	}
}
