using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationEvent : MonoBehaviour
{
	public void OnEnterLocation_Attic(LocationCard loc)
	{
		Player.Get().DecreaseSanity(1);
		GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "进入地点后：神智减1\n");
	}

	public void OnLocationAction_Miskatonic_University(LocationCard loc)
	{
		Player.Get().m_actionUsed += 1;

		var lstCards = GameLogic.Get().m_mainGameUI.m_lstCardChoice;
		int num = Mathf.Min(6, GameLogic.Get().m_lstPlayerCards.Count);
		lstCards.Clear();

		for (int i=0; i<num; ++i)
		{
			PlayerCard card = GameLogic.Get().m_lstPlayerCards[i].GetComponent<PlayerCard>();

			if(card.IsKeywordContain(Card.Keyword.Tome) || card.IsKeywordContain(Card.Keyword.Spell))
			{
				lstCards.Add(card);
			}
		}
		
		if(lstCards.Count > 0)
		{
			List<string> cardNames = new List<string>();
			lstCards.ForEach(card => { cardNames.Add(card.m_cardName); });
			GameLogic.Get().m_mainGameUI.m_chooseCardDropdown.ClearOptions();
			GameLogic.Get().m_mainGameUI.m_chooseCardDropdown.AddOptions(cardNames);
			GameLogic.Get().m_mainGameUI.m_chooseCardDropdown.gameObject.SetActive(true);

			GameLogic.Get().ShowHighlightCardExclusive(lstCards[0], false);
			GameLogic.Get().m_mainGameUI.m_confirmChooseCardBtn.gameObject.SetActive(true);
		}
		else
		{
			GameLogic.Shuffle(GameLogic.Get().m_lstPlayerCards);
			GameLogic.Get().OutputGameLog(string.Format("{0}使用了米斯卡塔尼克大学的地点行动：没搜索到对应卡牌！\n", Player.Get().m_investigatorCard.m_cardName));
		}
	}

	public void OnChooseCardChanged(Dropdown d)
	{
		var lstCards = GameLogic.Get().m_mainGameUI.m_lstCardChoice;

		if (GameLogic.Get().m_highlightCard != null)
		{
			GameLogic.Get().m_highlightCard.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
		}
		GameLogic.Get().ShowHighlightCardExclusive(lstCards[d.value], false);
	}

	public void OnButtonConfirmChooseCard()
	{
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = true;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = true;

		var lstCards = GameLogic.Get().m_mainGameUI.m_lstCardChoice;
		PlayerCard card = lstCards[GameLogic.Get().m_mainGameUI.m_chooseCardDropdown.value];
		Player.Get().AddHandCard(card.gameObject);

		GameLogic.Get().m_lstPlayerCards.Remove(card.gameObject);
		GameLogic.Shuffle(GameLogic.Get().m_lstPlayerCards);

		card.OnPointerExit(new UnityEngine.EventSystems.BaseEventData(null));
		GameLogic.Get().m_mainGameUI.m_confirmChooseCardBtn.gameObject.SetActive(false);
		GameLogic.Get().m_mainGameUI.m_chooseCardDropdown.gameObject.SetActive(false);
		GameLogic.Get().OutputGameLog(string.Format("{0}获取了手牌：{1}\n", Player.Get().m_investigatorCard.m_cardName, card.m_cardName));
	}
}
