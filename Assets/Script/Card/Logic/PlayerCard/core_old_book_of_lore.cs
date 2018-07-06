/********************************************************************
	created:	2018/07/03
	created:	3:7:2018   21:34
	author:		maval
	
	Note:		You can't use the card ability if there isn't at least 3 cards in your deck.	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class core_old_book_of_lore : PlayerCardLogic
{
	private string				m_cardAction = "<老旧的书>卡牌行动";
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
		int actionIndex = mainUI.GetActionDropdownItemIndex(m_cardAction);

		if (index == actionIndex)
		{
			GameLogic.Get().OutputGameLog(string.Format("{0}使用<老旧的书>的卡牌行动，", Player.Get().m_investigatorCard.m_cardName));

			Player.Get().m_currentAction.Push((PlayerAction)actionIndex);

			List<string> options = new List<string>();
			options.Add(GameLogic.Get().m_lstPlayerCards[0].GetComponent<Card>().m_cardName);
			options.Add(GameLogic.Get().m_lstPlayerCards[1].GetComponent<Card>().m_cardName);
			options.Add(GameLogic.Get().m_lstPlayerCards[2].GetComponent<Card>().m_cardName);

			mainUI.m_choiceDropdown.ClearOptions();
			mainUI.m_choiceDropdown.AddOptions(options);
			mainUI.m_choiceDropdown.RefreshShownValue();

			mainUI.m_choiceDropdown.gameObject.SetActive(true);
			mainUI.m_confirmChoiceBtn.gameObject.SetActive(true);
			mainUI.m_choiceMode = MainGame.ConfirmButtonMode.TextOnly;

			mainUI.m_lstChoiceEvent.Clear();
			mainUI.m_lstChoiceEvent.Add(new ChoiceEvent());
			mainUI.m_lstChoiceEvent.Add(new ChoiceEvent());
			mainUI.m_lstChoiceEvent.Add(new ChoiceEvent());

			mainUI.m_lstChoiceEvent[0].AddListener(new UnityAction<object>(OnSelectCard1));
			mainUI.m_lstChoiceEvent[1].AddListener(new UnityAction<object>(OnSelectCard2));
			mainUI.m_lstChoiceEvent[2].AddListener(new UnityAction<object>(OnSelectCard3));
		}
	}

	private void OnSelectCard1(object param)
	{
		_OnSelectCard(0);
	}

	private void OnSelectCard2(object param)
	{
		_OnSelectCard(1);
	}

	private void OnSelectCard3(object param)
	{
		_OnSelectCard(2);
	}

	private void _OnSelectCard(int index)
	{
		var card = GameLogic.Get().m_lstPlayerCards[index];
		GameLogic.Get().m_lstPlayerCards.RemoveAt(index);
		card.SetActive(true);

		GameLogic.Get().OutputGameLog(string.Format("获取了{0}\n", card.GetComponent<Card>().m_cardName));

		Player.Get().AddHandCard(card);
		GameLogic.Shuffle(GameLogic.Get().m_lstPlayerCards);

		var mainUI = GameLogic.Get().m_mainGameUI;
		Player.Get().ActionDone((PlayerAction)mainUI.GetActionDropdownItemIndex(m_cardAction));
	}

	private void Update()
	{
		var mainUI = GameLogic.Get().m_mainGameUI;
		mainUI.m_isActionEnable[(PlayerAction)mainUI.GetActionDropdownItemIndex(m_cardAction)] = !GetComponent<Card>().m_exhausted && GameLogic.Get().m_lstPlayerCards.Count >= 3;
	}
}
