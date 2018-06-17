/********************************************************************
	created:	2018/06/12
	created:	12:6:2018   19:28
	author:		maval
	
	Note:		1. Now hurt only investigator and enemy.
	TODO:		1. Each engaged enemy makes an attack of opportunity against you, and then the effects of the card resolve
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class core_dynamite_blast : PlayerCardLogic
{
	private UnityAction<int>	m_onValueChanged;

	public override void OnReveal(Card card)
	{
		var ui = GameLogic.Get().m_mainGameUI;
		Player.Get().m_currentAction = PlayerAction.NonStandardAction1;

		// ...................Seems like Unity's BUG.......................
		ScrollRect dropDownList = ui.m_targetDropdown.GetComponentInChildren<ScrollRect>();
		if (dropDownList != null)
		{
			GameObject.Destroy(dropDownList.gameObject);
		}
		ui.m_targetDropdown.ClearOptions();
		List<string> optionNames = new List<string>();

		var destList = Player.Get().m_currentLocation.m_lstDestinations;

		optionNames.Add("请选择爆炸地点...");
		optionNames.Add(Player.Get().m_currentLocation.m_cardName);
		destList.ForEach(dest => { optionNames.Add(dest.m_cardName); });

		ui.m_targetDropdown.AddOptions(optionNames);
		ui.m_targetDropdown.RefreshShownValue();
		ui.m_targetDropdown.value = 0;

		m_onValueChanged = new UnityAction<int>(OnTargetDropdownChanged);
		ui.m_targetDropdown.onValueChanged.AddListener(m_onValueChanged);

		ui.m_actionDropdown.gameObject.SetActive(false);
		ui.m_targetDropdown.gameObject.SetActive(true);
	}

	private void OnTargetDropdownChanged(int index)
	{
		if (index < 1)
		{
			return;
		}

		StartCoroutine(_OnTargetDropdownChanged(index));
	}

	private IEnumerator _OnTargetDropdownChanged(int index)
	{
		var ui = GameLogic.Get().m_mainGameUI;
		ui.m_targetDropdown.onValueChanged.RemoveListener(m_onValueChanged);
		ui.m_targetDropdown.gameObject.SetActive(false);
		ui.m_actionDropdown.gameObject.SetActive(true);

		LocationCard location;

		if (index == 1)
		{
			location = Player.Get().m_currentLocation;
		}
		else
		{
			location = Player.Get().m_currentLocation.m_lstDestinations[index - 2];
		}

		GameLogic.Get().OutputGameLog(string.Format("{0}在<{1}>打出了<炸药爆破>！\n", Player.Get().m_investigatorCard.m_cardName, location.m_cardName));

		if (index == 1)
		{
			Player.Get().DecreaseHealth(null, 3);
			GameLogic.Get().OutputGameLog(string.Format("{0}受到了3点伤害\n", Player.Get().m_investigatorCard.m_cardName));

			var engaged = Player.Get().GetEnemyCards();
			foreach (var enemy in engaged)
			{
				enemy.DecreaseHealth(3);
			}
		}

		location.m_lstCardsAtHere.ForEach((victim) =>
		{
			if (victim is EnemyCard)
			{
				EnemyCard enemy = victim as EnemyCard;
				enemy.DecreaseHealth(3);
			}
		});

		yield return new WaitUntil(() => GameLogic.Get().m_currentTiming == EventTiming.None);
		Player.Get().m_currentAction = PlayerAction.None;
	}
}
