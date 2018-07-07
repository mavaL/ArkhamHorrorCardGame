/********************************************************************
	created:	2018/06/21
	created:	21:6:2018   7:59
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class core_roland_dot38_special : PlayerCardLogic
{
	public int					m_bullet;

	private string				m_cardAction = "<罗兰德的.38特制手枪>卡牌行动";
	private UnityAction<int>	m_onCardAction;
	private UnityAction<EnemyCard>	m_afterEnemyDamaged;
	private int					m_bonusCombat;

	public override void OnReveal(Card card)
	{
		m_onCardAction = new UnityAction<int>(OnCardAction);
		m_afterEnemyDamaged = new UnityAction<EnemyCard>(AfterEnemyDamaged);

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
			m_bonusCombat = Player.Get().m_currentLocation.m_clues > 0 ? 3 : 1;

			Player.Get().m_investigatorCard.m_combat += m_bonusCombat;
			Player.Get().m_attackDamage += 1;

			GameLogic.Get().m_afterEnemyDamagedEvent.AddListener(m_afterEnemyDamaged);
			GameLogic.Get().m_mainGameUI.m_actionDropdown.value = (int)PlayerAction.Fight;
		}
	}

	private void AfterEnemyDamaged(EnemyCard target)
	{
		GameLogic.Get().m_afterEnemyDamagedEvent.RemoveListener(m_afterEnemyDamaged);
		Player.Get().m_investigatorCard.m_combat -= m_bonusCombat;
		Player.Get().m_attackDamage -= 1;
		m_bullet -= 1;
		m_bonusCombat = 0;

		UnityEngine.Assertions.Assert.IsTrue(m_bullet>=0, "Assert failed in core_roland_dot38_special.AfterEnemyDamaged()!!!");
	}

	private void Update()
	{
		if(m_isActive)
		{
			var ui = GameLogic.Get().m_mainGameUI;
			ui.m_isActionEnable[(PlayerAction)ui.GetActionDropdownItemIndex(m_cardAction)] = Player.Get().IsAnyEnemyToFightWith() && m_bullet > 0;
		}
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
