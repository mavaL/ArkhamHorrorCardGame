/********************************************************************
	created:	2018/07/06
	created:	6:7:2018   0:28
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class core_flashlight : PlayerCardLogic
{
	public int						m_supply;
	private int						m_locationShroud;

	private string					m_cardAction = "<手电筒>卡牌行动";
	private UnityAction<int>		m_onCardAction;
	private UnityAction<int, Card>	m_afterSkillTest;

	public override void OnReveal(Card card)
	{
		m_onCardAction = new UnityAction<int>(OnCardAction);
		m_afterSkillTest = new UnityAction<int, Card>(AfterSkillTest);

		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_actionDropdown.options.Add(new Dropdown.OptionData(m_cardAction));
		ui.m_actionDropdown.onValueChanged.AddListener(m_onCardAction);

		m_isActive = true;
		Player.Get().m_currentAction.Pop();
	}

	public override void OnDiscard(Card card)
	{
		if(m_isActive)
		{
			m_isActive = false;

			var ui = GameLogic.Get().m_mainGameUI;
			ui.m_actionDropdown.onValueChanged.RemoveListener(m_onCardAction);
			ui.m_actionDropdown.options.RemoveAt(ui.GetActionDropdownItemIndex(m_cardAction));
		}
	}

	private void OnCardAction(int index)
	{
		var ui = GameLogic.Get().m_mainGameUI;

		if (index == ui.GetActionDropdownItemIndex(m_cardAction))
		{
			m_locationShroud = Player.Get().m_currentLocation.m_shroud;
			Player.Get().m_currentLocation.m_shroud -= 2;
			Player.Get().m_currentLocation.m_shroud = UnityEngine.Mathf.Max(0, Player.Get().m_currentLocation.m_shroud);

			GameLogic.Get().m_afterSkillTest.AddListener(m_afterSkillTest);
			GameLogic.Get().m_mainGameUI.m_actionDropdown.value = (int)PlayerAction.Investigate;
		}
	}

	private void AfterSkillTest(int result, Card target)
	{
		GameLogic.Get().m_afterSkillTest.RemoveListener(m_afterSkillTest);
		Player.Get().m_currentLocation.m_shroud = m_locationShroud;
		m_supply -= 1;

		UnityEngine.Assertions.Assert.IsTrue(m_supply >= 0, "Assert failed in core_dot45_automatic.AfterEnemyDamaged()!!!");
	}

	private void Update()
	{
		if(m_isActive)
		{
			var ui = GameLogic.Get().m_mainGameUI;
			ui.m_isActionEnable[(PlayerAction)ui.GetActionDropdownItemIndex(m_cardAction)] = Player.Get().m_currentLocation.m_clues > 0 && m_supply > 0;
		}
	}

	public override int GetAssetResource()
	{
		return m_supply;
	}

	public override bool HasUseLimit()
	{
		return true;
	}
}
