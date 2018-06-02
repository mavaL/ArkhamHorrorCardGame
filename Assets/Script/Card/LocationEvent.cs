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

	public void OnEnterLocation_Cellar(LocationCard loc)
	{
		Player.Get().DecreaseHealth(1);
		GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "进入地点后：生命减1\n");
	}

	public void OnLocationAction_Miskatonic_University(LocationCard loc)
	{
		Player.Get().ActionDone();

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
			GameLogic.Get().m_mainGameUI.m_choiceDropdown.ClearOptions();
			GameLogic.Get().m_mainGameUI.m_choiceDropdown.AddOptions(cardNames);
			GameLogic.Get().m_mainGameUI.m_movementDropdown.RefreshShownValue();

			GameLogic.Get().m_mainGameUI.m_choiceDropdown.gameObject.SetActive(true);

			GameLogic.Get().ShowHighlightCardExclusive(lstCards[0], false);
			GameLogic.Get().m_mainGameUI.m_confirmChoiceBtn.gameObject.SetActive(true);

			GameLogic.Get().m_mainGameUI.m_choiceMode = MainGame.ConfirmButtonMode.GainCard;
		}
		else
		{
			GameLogic.Shuffle(GameLogic.Get().m_lstPlayerCards);
			GameLogic.Get().OutputGameLog(string.Format("{0}使用了米斯卡塔尼克大学的地点行动：没搜索到对应卡牌！\n", Player.Get().m_investigatorCard.m_cardName));
		}
	}
}
