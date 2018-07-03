/********************************************************************
	created:	2018/06/18
	created:	18:6:2018   21:01
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class core_dot45_automatic : PlayerCardLogic
{
	public int					m_bullet;

	private PlayerAction		m_cardAction;
	private UnityAction<int>	m_onCardAction;
	private UnityAction			m_afterEnemyDamaged;

	public override void OnReveal(Card card)
	{
		m_onCardAction = new UnityAction<int>(OnCardAction);
		m_afterEnemyDamaged = new UnityAction(AfterEnemyDamaged);

		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_actionDropdown.options.Add(new Dropdown.OptionData("<.45自动手枪>卡牌行动"));
		m_cardAction = (PlayerAction)ui.m_actionDropdown.options.Count - 1;
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
			ui.m_actionDropdown.options.RemoveAt((int)m_cardAction);
		}
	}

	private void OnCardAction(int index)
	{
		if (index == (int)m_cardAction)
		{
			Player.Get().m_investigatorCard.m_combat += 1;
			Player.Get().m_attackDamage += 1;

			GameLogic.Get().m_afterEnemyDamagedEvent.AddListener(m_afterEnemyDamaged);
			GameLogic.Get().m_mainGameUI.m_actionDropdown.value = (int)PlayerAction.Fight;
		}
	}

	private void AfterEnemyDamaged()
	{
		GameLogic.Get().m_afterEnemyDamagedEvent.RemoveListener(m_afterEnemyDamaged);
		Player.Get().m_investigatorCard.m_combat -= 1;
		Player.Get().m_attackDamage -= 1;
		m_bullet -= 1;

		UnityEngine.Assertions.Assert.IsTrue(m_bullet>=0, "Assert failed in core_dot45_automatic.AfterEnemyDamaged()!!!");
	}

	private void Update()
	{
		GameLogic.Get().m_mainGameUI.m_isActionEnable[m_cardAction] = Player.Get().IsAnyEnemyToFightWith() && m_bullet > 0;
	}

	public override void AddAssetResource(int num)
	{
		m_bullet += num;
	}

	public override int GetAssetResource()
	{
		return m_bullet;
	}

	public override bool HasUseLimit()
	{
		return true;
	}
}
