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
	private PlayerAction		m_cardAction;
	private UnityAction<int>	m_onCardAction;
	private UnityAction<int>	m_onTargetDropdownChanged;

	public override void OnReveal(Card card)
	{
		Player.Get().m_investigatorCard.m_combat += 1;

		m_onCardAction = new UnityAction<int>(OnCardAction);
		m_onTargetDropdownChanged = new UnityAction<int>(OnTargetDropdownChanged);

		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_actionDropdown.options.Add(new Dropdown.OptionData("<疲惫的巡警>卡牌行动"));
		m_cardAction = (PlayerAction)ui.m_actionDropdown.options.Count - 1;
		ui.m_actionDropdown.onValueChanged.AddListener(m_onCardAction);
	}

	public override void OnDiscard(Card card)
	{
		Player.Get().m_investigatorCard.m_combat -= 1;
	}

	private void OnCardAction(int index)
	{
		if (index == (int)m_cardAction)
		{
			Player.Get().m_currentAction = PlayerAction.BeatcopCardAction;

			var ui = GameLogic.Get().m_mainGameUI;
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
		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_actionDropdown.onValueChanged.RemoveListener(m_onCardAction);
		ui.m_targetDropdown.onValueChanged.RemoveListener(m_onTargetDropdownChanged);
		ui.m_actionDropdown.options.RemoveAt((int)m_cardAction);

		var enemy = Player.Get().GetEnemyCards()[index - 1];
		enemy.DecreaseHealth(1);

		yield return new WaitUntil(() => GameLogic.Get().m_currentTiming == EventTiming.None);

		GetComponent<Card>().Discard(true);
	
		ui.m_actionDropdown.gameObject.SetActive(true);
		ui.m_targetDropdown.gameObject.SetActive(false);
		
		Player.Get().m_currentAction = PlayerAction.None;
		Player.Get().ActionDone(PlayerAction.BeatcopCardAction);
	}

	private void Update()
	{
		GameLogic.Get().m_mainGameUI.m_isActionEnable[m_cardAction] = GameLogic.Get().IsAnyEnemyToFightWith();
	}
}
