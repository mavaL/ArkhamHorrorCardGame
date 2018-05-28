using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationEvent : MonoBehaviour
{
	public void OnEnterLocation_Attic(LocationCard loc)
	{
		Player.Get().DecreaseSanity(1);
		GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "进入地点后：神智减1\n");
	}

	private List<PlayerCard>	m_lstCardChoice = new List<PlayerCard>();

	public void OnLocationAction_Miskatonic_University(LocationCard loc)
	{
		int num = Mathf.Min(6, GameLogic.Get().m_lstPlayerCards.Count);

		for(int i=0; i<num; ++i)
		{
			PlayerCard card = GameLogic.Get().m_lstPlayerCards[i].GetComponent<PlayerCard>();

			if(card.IsKeywordContain(Card.Keyword.Tome) || card.IsKeywordContain(Card.Keyword.Spell))
			{
				m_lstCardChoice.Add(card);
			}
		}
		
		if(m_lstCardChoice.Count > 0)
		{
			List<string> cardNames = new List<string>();
			m_lstCardChoice.ForEach(card => { cardNames.Add(card.m_cardName); });
			GameLogic.Get().m_mainGameUI.m_chooseCardDropdown.AddOptions(cardNames);

			GameLogic.Get().ShowHighlightCardExclusive(m_lstCardChoice[0], false);
			GameLogic.Get().m_mainGameUI.m_confirmChooseCardBtn.gameObject.SetActive(true);
		}
		else
		{
			GameLogic.Get().OutputGameLog(string.Format("{0}使用了米斯卡塔尼克大学的地点行动：没搜索到对应卡牌！\n", Player.Get().m_investigatorCard.m_cardName));
		}
	}

	public void OnButtonConfirmChooseCard()
	{
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = true;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = true;

		PlayerCard card = m_lstCardChoice[GameLogic.Get().m_mainGameUI.m_chooseCardDropdown.value];

		GameLogic.Get().m_mainGameUI.m_confirmChooseCardBtn.gameObject.SetActive(false);

		GameLogic.Get().OutputGameLog(string.Format("{0}获取了手牌：{1}\n", Player.Get().m_investigatorCard.m_cardName, card.m_cardName));
	}
}
