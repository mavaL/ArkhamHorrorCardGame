using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class core_gathering : scenario_base
{
	public core_gathering()
	{
		m_startLocation = (GameObject)GameObject.Instantiate(Resources.Load("CardPrefabs/Locations/core_location_study"));
	}

	public override void ShowPlayInfo()
	{
		LocationCard card = m_revealedLocations[0].GetComponent<LocationCard>();

		var textComp = GameObject.Find("ClueInfo").GetComponent<Text>();
		textComp.text  = string.Format("当前资源：<color=orange>{0}</color>\n<color=red>各地点的线索：</color>\n书房： <color=green>{1}</color>\n", 
			Player.Get().m_resources,
			card.m_clues);
	}

	public override int GetChaosTokenEffect(ChaosBag.ChaosTokenType t)
	{
		if(GameLogic.Get().m_difficulty == GameDifficulty.Easy ||
		GameLogic.Get().m_difficulty == GameDifficulty.Normal )
		{
			switch (t)
			{
				case ChaosBag.ChaosTokenType.Skully:
					return 0 - Player.Get().m_currentLocation.HowManyEnemyContainTheKeyword(EnemyCard.Keyword.Ghoul);
				case ChaosBag.ChaosTokenType.Cultist:
					return -1;
				case ChaosBag.ChaosTokenType.Tablet:
					if (Player.Get().m_currentLocation.HowManyEnemyContainTheKeyword(EnemyCard.Keyword.Ghoul) > 0)
					{
						Player.Get().DecreaseHealth();
						GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "结算混沌标记：受到1点伤害\n");
					}
					return -2;
                default:
					break;
			}
		}
		else
		{
			Debug.Log("Hard/Expert difficulty not impl in GetChaosTokenEffect()!!");
		}
		
		Assert.IsTrue(false, "Assert failed in GetChaosTokenEffect()!!");
		return -1;
	}

	public override void AfterSkillTestFailed(ChaosBag.ChaosTokenType t)
	{
		if (GameLogic.Get().m_difficulty == GameDifficulty.Easy ||
		GameLogic.Get().m_difficulty == GameDifficulty.Normal)
		{
			switch (t)
			{
				case ChaosBag.ChaosTokenType.Cultist:
					Player.Get().DecreaseSanity();
					GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "结算混沌标记：神智减1\n");
					break;
				default:
					break;

			}
		}
		else
		{
			Debug.Log("Hard/Expert difficulty not impl in AfterSkillTest()!!");
		}
	}
}
