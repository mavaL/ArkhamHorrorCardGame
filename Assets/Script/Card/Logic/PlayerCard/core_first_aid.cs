/********************************************************************
	created:	2018/06/19
	created:	19:6:2018   22:55
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class core_first_aid : PlayerCardLogic
{
	public int					m_supply;

	private string				m_cardAction = "<急救>卡牌行动";
	private UnityAction<int>	m_onCardAction;

	public override void OnReveal(Card card)
	{
		m_onCardAction = new UnityAction<int>(OnCardAction);

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
		var mainUI = GameLogic.Get().m_mainGameUI;

		if (index == mainUI.GetActionDropdownItemIndex(m_cardAction))
		{
			UnityEngine.Assertions.Assert.IsTrue(m_supply > 0, "Supply <= 0 in core_first_aid.OnCardAction()!!!");
			m_supply -= 1;

			bool bHurt = Player.Get().GetHp() < Player.Get().m_investigatorCard.m_health;
			bool bHorror = Player.Get().GetSan() < Player.Get().m_investigatorCard.m_sanity;

			UnityEngine.Assertions.Assert.IsTrue(bHurt || bHorror, "Investigator is full HP/SAN in core_first_aid.OnCardAction()!!!");

			mainUI.m_choiceDropdown.ClearOptions();

			mainUI.m_choiceDropdown.gameObject.SetActive(true);
			mainUI.m_confirmChoiceBtn.gameObject.SetActive(true);
			mainUI.m_choiceMode = MainGame.ConfirmButtonMode.TextOnly;

			mainUI.m_lstChoiceEvent.Clear();

			List<string> options = new List<string>();
			if (bHurt)
			{
				options.Add("治疗1伤害");
				mainUI.m_lstChoiceEvent.Add(new ChoiceEvent());
				mainUI.m_lstChoiceEvent[0].AddListener(new UnityAction<object>(OnHealOneHealth));
			}
			if (bHorror)
			{
				options.Add("治疗1恐怖");
				mainUI.m_lstChoiceEvent.Add(new ChoiceEvent());
				mainUI.m_lstChoiceEvent[1].AddListener(new UnityAction<object>(OnHealOneSanity));
			}

			mainUI.m_choiceDropdown.AddOptions(options);
			mainUI.m_choiceDropdown.RefreshShownValue();
		}
	}

	private void OnHealOneHealth(object param)
	{
		Player.Get().HealHealth(1);

		if (m_supply == 0)
		{
			GameLogic.Get().OutputGameLog("<急救>供应为0，已自动丢弃\n");
			GetComponent<Card>().Discard();
		}

		var mainUI = GameLogic.Get().m_mainGameUI;
		Player.Get().ActionDone((PlayerAction)mainUI.GetActionDropdownItemIndex(m_cardAction));
	}

	private void OnHealOneSanity(object param)
	{
		Player.Get().HealSanity(1);

		if (m_supply == 0)
		{
			GameLogic.Get().OutputGameLog("<急救>供应为0，已自动丢弃\n");
			GetComponent<Card>().Discard();
		}

		var mainUI = GameLogic.Get().m_mainGameUI;
		Player.Get().ActionDone((PlayerAction)mainUI.GetActionDropdownItemIndex(m_cardAction));
	}

	private void Update()
	{
		if(m_isActive)
		{
			var mainUI = GameLogic.Get().m_mainGameUI;
			mainUI.m_isActionEnable[(PlayerAction)mainUI.GetActionDropdownItemIndex(m_cardAction)] = Player.Get().GetHp() < Player.Get().m_investigatorCard.m_health || Player.Get().GetSan() < Player.Get().m_investigatorCard.m_sanity;
		}
	}

	public override void AddAssetResource(int num)
	{
		m_supply += num;
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
