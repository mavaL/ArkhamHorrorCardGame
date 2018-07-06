/********************************************************************
	created:	2018/06/16
	created:	16:6:2018   11:19
	author:		maval
	
	TODO:		1. Discard ability should works on disengaged enemy	at player's location.
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class core_beat_cop : PlayerCardLogic
{
	private string				m_cardAction = "<疲惫的巡警>卡牌行动";
	private UnityAction<int>	m_onCardAction;
	private UnityAction<int>	m_onTargetDropdownChanged;

	public override void OnReveal(Card card)
	{
		Player.Get().m_investigatorCard.m_combat += 1;

		m_onCardAction = new UnityAction<int>(OnCardAction);
		m_onTargetDropdownChanged = new UnityAction<int>(OnTargetDropdownChanged);

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
			Player.Get().m_investigatorCard.m_combat -= 1;
			m_isActive = false;
		}
	}

	private void OnCardAction(int index)
	{
		var ui = GameLogic.Get().m_mainGameUI;

		if (index == ui.GetActionDropdownItemIndex(m_cardAction))
		{
			Player.Get().m_currentAction.Push(PlayerAction.BeatcopCardAction);

			ui.m_actionDropdown.gameObject.SetActive(false);
			ui.m_targetDropdown.gameObject.SetActive(true);
			ui.UpdateTargetDropdown();
			ui.m_targetDropdown.onValueChanged.AddListener(m_onTargetDropdownChanged);
		}
	}

	private void OnTargetDropdownChanged(int index)
	{
		if (index < 1)
		{
			return;
		}

		StartCoroutine(_OnChooseTarget(index));
	}

	private IEnumerator _OnChooseTarget(int index)
	{
		GameLogic.Get().OutputGameLog("<疲惫的巡警>执行了卡牌行动\n");

		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_actionDropdown.onValueChanged.RemoveListener(m_onCardAction);
		ui.m_targetDropdown.onValueChanged.RemoveListener(m_onTargetDropdownChanged);
		ui.m_actionDropdown.options.RemoveAt(ui.GetActionDropdownItemIndex(m_cardAction));

		ui.m_targetDropdown.gameObject.SetActive(false);

		var enemy = Player.Get().GetEnemyFromEngagedOrLocation(index - 1);
		enemy.DecreaseHealth(1);

		yield return new WaitUntil(() => GameLogic.Get().m_currentTiming == EventTiming.None);

		GetComponent<Card>().Discard();

		Player.Get().ActionDone(PlayerAction.BeatcopCardAction, false);
	}

	private void Update()
	{
		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_isActionEnable[(PlayerAction)ui.GetActionDropdownItemIndex(m_cardAction)] = Player.Get().IsAnyEnemyToFightWith();
	}
}
