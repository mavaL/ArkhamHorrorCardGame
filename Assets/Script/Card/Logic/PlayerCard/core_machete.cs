/********************************************************************
	created:	2018/06/20
	created:	20:6:2018   8:02
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class core_machete : PlayerCardLogic
{
	private PlayerAction			m_cardAction;
	private UnityAction<int>		m_onCardAction;
	private UnityAction				m_afterEnemyDamaged;
	private UnityAction<EnemyCard>	m_beforeEnemyDamaged;
	private bool					m_bBonus = false;

	public override void OnReveal(Card card)
	{
		m_onCardAction = new UnityAction<int>(OnCardAction);
		m_beforeEnemyDamaged = new UnityAction<EnemyCard>(BeforeEnemyDamaged);
		m_afterEnemyDamaged = new UnityAction(AfterEnemyDamaged);

		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_actionDropdown.options.Add(new Dropdown.OptionData("<弯刀>卡牌行动"));
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
			GameLogic.Get().m_beforeEnemyDamagedEvent.AddListener(m_beforeEnemyDamaged);
			GameLogic.Get().m_mainGameUI.m_actionDropdown.value = (int)PlayerAction.Fight;
		}
	}

	private void BeforeEnemyDamaged(EnemyCard enemy)
	{
		GameLogic.Get().m_beforeEnemyDamagedEvent.RemoveListener(m_beforeEnemyDamaged);
		GameLogic.Get().m_afterEnemyDamagedEvent.AddListener(m_afterEnemyDamaged);

		Player.Get().m_investigatorCard.m_combat += 1;

		if(enemy.m_engaged && Player.Get().GetEnemyCards().Count == 1)
		{
			UnityEngine.Assertions.Assert.IsTrue(Player.Get().GetEnemyCards()[0].m_cardName == enemy.m_cardName, "Assert failed in core_machete.BeforeEnemyDamaged()!!!");

			Player.Get().m_attackDamage += 1;
			m_bBonus = true;
		}
	}

	private void AfterEnemyDamaged()
	{
		GameLogic.Get().m_afterEnemyDamagedEvent.RemoveListener(m_afterEnemyDamaged);

		Player.Get().m_investigatorCard.m_combat -= 1;
		if(m_bBonus)
		{
			Player.Get().m_attackDamage -= 1;
			m_bBonus = false;
		}
	}

	private void Update()
	{
		GameLogic.Get().m_mainGameUI.m_isActionEnable[m_cardAction] = Player.Get().IsAnyEnemyToFightWith();
	}
}
