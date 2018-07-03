/********************************************************************
	created:	2018/07/03
	created:	3:7:2018   22:15
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class core_research_librarian : PlayerCardLogic
{
	public override void OnReveal(Card card)
	{
		List<string> options = new List<string>();
		var cards = GameLogic.Get().m_lstPlayerCards;
		var mainUI = GameLogic.Get().m_mainGameUI;
		mainUI.m_lstChoiceEvent.Clear();

		foreach (var playerCard in cards)
		{
			Card pc = playerCard.GetComponent<Card>();
			if (pc.IsKeywordContain(Card.Keyword.Tome))
			{
				options.Add(pc.m_cardName);
				mainUI.m_lstChoiceEvent.Add(new ChoiceEvent(pc));
				mainUI.m_lstChoiceEvent[mainUI.m_lstChoiceEvent.Count - 1].AddListener(new UnityAction<object>(OnSelectTome));
			}
		}

		if (options.Count == 0)
		{
			GameLogic.Get().OutputGameLog("由于玩家牌组没有古籍牌，<研究馆员>没有发挥效果\n");
			Player.Get().m_currentAction.Pop();
			return;
		}

		mainUI.m_choiceDropdown.ClearOptions();
		mainUI.m_choiceDropdown.AddOptions(options);
		mainUI.m_choiceDropdown.RefreshShownValue();

		mainUI.m_choiceDropdown.gameObject.SetActive(true);
		mainUI.m_confirmChoiceBtn.gameObject.SetActive(true);
		mainUI.m_choiceMode = MainGame.ConfirmButtonMode.TextOnly;

		mainUI.m_confirmChoiceText.gameObject.SetActive(true);
		mainUI.m_confirmChoiceText.text = "<研究馆员>：获取1古籍牌";
	}

	private void OnSelectTome(object param)
	{
		Card card = param as Card;
		GameLogic.Get().m_lstPlayerCards.Remove(card.gameObject);
		card.gameObject.SetActive(true);

		GameLogic.Get().OutputGameLog(string.Format("<研究馆员>让{0}获取了<{1}>\n", Player.Get().m_investigatorCard.m_cardName, card.m_cardName));

		Player.Get().AddHandCard(card.gameObject);
		GameLogic.Shuffle(GameLogic.Get().m_lstPlayerCards);

		Player.Get().m_currentAction.Pop();
		GameLogic.Get().m_mainGameUI.m_confirmChoiceText.gameObject.SetActive(false);
	}
}
