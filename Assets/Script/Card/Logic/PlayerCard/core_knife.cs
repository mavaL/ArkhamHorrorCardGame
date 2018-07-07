/********************************************************************
	created:	2018/07/06
	created:	6:7:2018   8:05
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class core_knife : PlayerCardLogic
{
	private string				m_cardAction1 = "<小刀>卡牌行动1";
	private string				m_cardAction2 = "<小刀>卡牌行动2";
	private UnityAction<int>	m_onCardAction1;
	private UnityAction<int>	m_onCardAction2;
	private UnityAction<EnemyCard>	m_afterEnemyDamaged1;
	private UnityAction<EnemyCard> m_afterEnemyDamaged2;

	public override void OnReveal(Card card)
	{
		m_onCardAction1 = new UnityAction<int>(OnCardAction1);
		m_onCardAction2 = new UnityAction<int>(OnCardAction2);
		m_afterEnemyDamaged1 = new UnityAction<EnemyCard>(AfterEnemyDamaged1);
		m_afterEnemyDamaged2 = new UnityAction<EnemyCard>(AfterEnemyDamaged2);

		var ui = GameLogic.Get().m_mainGameUI;

		ui.m_actionDropdown.options.Add(new Dropdown.OptionData(m_cardAction1));
		ui.m_actionDropdown.onValueChanged.AddListener(m_onCardAction1);

		ui.m_actionDropdown.options.Add(new Dropdown.OptionData(m_cardAction2));
		ui.m_actionDropdown.onValueChanged.AddListener(m_onCardAction2);

		m_isActive = true;
		Player.Get().m_currentAction.Pop();
	}

	public override void OnDiscard(Card card)
	{
		if(m_isActive)
		{
			var ui = GameLogic.Get().m_mainGameUI;
			int index1 = ui.GetActionDropdownItemIndex(m_cardAction1);
			int index2 = ui.GetActionDropdownItemIndex(m_cardAction2);
			ui.m_actionDropdown.options.RemoveAt(index1);
			ui.m_actionDropdown.options.RemoveAt(index2);

			m_isActive = false;
		}
	}

	private void OnCardAction1(int index)
	{
		if (index == GameLogic.Get().m_mainGameUI.GetActionDropdownItemIndex(m_cardAction1))
		{
			GameLogic.Get().OutputGameLog(string.Format("{0}使用了<小刀>的卡牌行动1\n", Player.Get().m_investigatorCard.m_cardName));

			Player.Get().m_investigatorCard.m_combat += 1;

			GameLogic.Get().m_afterEnemyDamagedEvent.AddListener(m_afterEnemyDamaged1);
			GameLogic.Get().m_mainGameUI.m_actionDropdown.value = (int)PlayerAction.Fight;
		}
	}

	private void OnCardAction2(int index)
	{
		if (index == GameLogic.Get().m_mainGameUI.GetActionDropdownItemIndex(m_cardAction2))
		{
			GameLogic.Get().OutputGameLog(string.Format("{0}使用了<小刀>的卡牌行动2\n", Player.Get().m_investigatorCard.m_cardName));

			Player.Get().m_investigatorCard.m_combat += 2;
			Player.Get().m_attackDamage += 1;

			GameLogic.Get().m_afterEnemyDamagedEvent.AddListener(m_afterEnemyDamaged2);
			GameLogic.Get().m_mainGameUI.m_actionDropdown.value = (int)PlayerAction.Fight;
		}
	}

	private void AfterEnemyDamaged1(EnemyCard target)
	{
		GameLogic.Get().m_afterEnemyDamagedEvent.RemoveListener(m_afterEnemyDamaged1);
		Player.Get().m_investigatorCard.m_combat -= 1;
	}

	private void AfterEnemyDamaged2(EnemyCard target)
	{
		GameLogic.Get().m_afterEnemyDamagedEvent.RemoveListener(m_afterEnemyDamaged2);
		Player.Get().m_investigatorCard.m_combat -= 2;
		Player.Get().m_attackDamage -= 1;

		GameLogic.Get().OutputGameLog("<小刀>被丢弃\n");

		GetComponent<PlayerCard>().Discard();
	}

	private void Update()
	{
		if(m_isActive)
		{
			PlayerAction action1 = (PlayerAction)GameLogic.Get().m_mainGameUI.GetActionDropdownItemIndex(m_cardAction1);
			PlayerAction action2 = (PlayerAction)GameLogic.Get().m_mainGameUI.GetActionDropdownItemIndex(m_cardAction2);

			GameLogic.Get().m_mainGameUI.m_isActionEnable[action1] = Player.Get().IsAnyEnemyToFightWith();
			GameLogic.Get().m_mainGameUI.m_isActionEnable[action2] = Player.Get().IsAnyEnemyToFightWith();
		}
	}
}
