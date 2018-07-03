/********************************************************************
	created:	2018/06/27
	created:	27:6:2018   21:23
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_mind_over_matter : PlayerCardLogic
{
	private UnityAction<SkillType>	m_beforeSkillTest;
	private UnityAction<int, Card>	m_afterSkillTest;
	private UnityAction				m_onRoundEnd;

	public override void OnReveal(Card card)
	{
		GameLogic.Get().OutputGameLog(string.Format("{0}打出了<心如电转>\n", Player.Get().m_investigatorCard.m_cardName));

		m_beforeSkillTest = new UnityAction<SkillType>(BeforeSkillTest);
		m_afterSkillTest = new UnityAction<int, Card>(AfterSkillTest);
		m_onRoundEnd = new UnityAction(OnRoundEnd);

		GameLogic.Get().m_mainGameUI.m_roundEndEvent.AddListener(m_onRoundEnd);
		GameLogic.Get().m_beforeSkillTest.AddListener(m_beforeSkillTest);

		Player.Get().m_currentAction.Pop();
	}

	private void BeforeSkillTest(SkillType type)
	{
		if(type == SkillType.Combat || type == SkillType.Agility)
		{
			List<string> options = new List<string>();
			options.Add("<心如电转>：用知识替代进行本次检定");
			options.Add("不替代");

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

			mainUI.m_lstChoiceEvent[0].AddListener(new UnityAction<object>(OnConfirmUseAbility));
			mainUI.m_lstChoiceEvent[1].AddListener(new UnityAction<object>(OnConfirmNotUseAbility));
			GameLogic.Get().m_currentTiming = EventTiming.BeforeSkillTest;
		}
	}

	private void OnConfirmUseAbility(object param)
	{
		GameLogic.Get().m_currentScenario.m_skillTest.m_replaceSkillTest = SkillType.Intellect;
		GameLogic.Get().m_afterSkillTest.AddListener(m_afterSkillTest);
		GameLogic.Get().m_currentTiming = EventTiming.None;
	}

	private void OnConfirmNotUseAbility(object param)
	{
		GameLogic.Get().m_currentTiming = EventTiming.None;
	}

	private void AfterSkillTest(int result, Card target)
	{
		GameLogic.Get().m_currentScenario.m_skillTest.m_replaceSkillTest = SkillType.None;
		GameLogic.Get().m_afterSkillTest.RemoveListener(m_afterSkillTest);
	}

	private void OnRoundEnd()
	{
		GameLogic.Get().m_mainGameUI.m_roundEndEvent.RemoveListener(m_onRoundEnd);
		GameLogic.Get().m_beforeSkillTest.RemoveListener(m_beforeSkillTest);
		GetComponent<PlayerCard>().Discard();
	}
}
