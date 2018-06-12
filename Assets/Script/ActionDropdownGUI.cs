using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionDropdownGUI : MonoBehaviour
{

	private void Start()
	{
		var ui = GameLogic.Get().m_mainGameUI;
		PlayerAction action = (PlayerAction)transform.GetSiblingIndex() - 1;
		var item = GetComponent<Toggle>();

		switch (action)
		{
			case PlayerAction.Move:
				item.interactable = Player.Get().m_currentLocation.m_lstDestinations.Count > 0 && ui.m_isActionEnable[PlayerAction.Move];
				break;
			case PlayerAction.Investigate:
				item.interactable = Player.Get().m_currentLocation.m_clues > 0 && ui.m_isActionEnable[PlayerAction.Investigate];
				break;
			case PlayerAction.Evade:
				item.interactable = Player.Get().GetEnemyCards().Count > 0 && ui.m_isActionEnable[PlayerAction.Evade];
				break;
			case PlayerAction.Fight:
				item.interactable = Player.Get().GetEnemyCards().Count > 0 && ui.m_isActionEnable[PlayerAction.Fight];
				break;
			case PlayerAction.DrawOneCard:
				item.interactable = ui.m_isActionEnable[PlayerAction.DrawOneCard];
				break;
			case PlayerAction.GainOneResource:
				item.interactable = ui.m_isActionEnable[PlayerAction.GainOneResource];
				break;
			case PlayerAction.PlayCard:
				item.interactable = Player.Get().CanPlayAnyHandCard() && ui.m_isActionEnable[PlayerAction.PlayCard];
				break;
			case PlayerAction.NonStandardAction1:
			case PlayerAction.NonStandardAction2:
			case PlayerAction.NonStandardAction3:
			case PlayerAction.NonStandardAction4:
			case PlayerAction.NonStandardAction5:
			case PlayerAction.NonStandardAction6:
			case PlayerAction.NonStandardAction7:
			case PlayerAction.NonStandardAction8:
			case PlayerAction.NonStandardAction9:
			case PlayerAction.NonStandardAction10:
				item.interactable = ui.m_isActionEnable[action];
				break;
		}
	}
}
