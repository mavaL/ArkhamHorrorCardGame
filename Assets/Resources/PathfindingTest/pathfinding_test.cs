using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class pathfinding_test : scenario_base
{
	void Awake()
	{
		InstantiateCards();

		GameLogic.Get().m_currentScenario = this;
	}

	void Start()
	{
		m_startLocation = Instantiate(m_startLocation);
		GameLogic.Get().PlayerEnterLocation(m_startLocation, false);
		GameLogic.DockCard(m_startLocation, GameObject.Find("StartLocation"));

		GameLogic.Get().OutputGameLog(Player.Get().m_investigatorCard.m_cardName + "进入了场景\n章节1开始\n");

		GameLogic.Get().m_mainGameUI.OnButtonEnterInvestigationPhase();
	}

	public override void ShowPlayInfo()
	{
		var ui = GameLogic.Get().m_mainGameUI;

		ui.m_statsInfoText.text  = string.Format(
			"血量：<color=green>{0}</color> 神智：<color=green>{1}</color> 意志：<color=green>{2}</color> 知识：<color=green>{3}</color> 力量：<color=green>{4}</color> 敏捷：<color=green>{5}</color>\n" +
			"剩余行动：<color=green>{6}</color>\n" +
			"持有资源：<color=green>{7}</color>\n" +
			"持有线索：<color=green>{8}</color>\n" +
			"章节已推进标记数：<color=green>{9}</color>\n" +
			"恶兆已逼近标记数：<color=red>{10}</color>\n" +
			"各地点的线索：\n",
			Player.Get().GetHp(),
			Player.Get().GetSan(),
			Player.Get().m_investigatorCard.m_willPower,
			Player.Get().m_investigatorCard.m_intellect,
			Player.Get().m_investigatorCard.m_combat,
			Player.Get().m_investigatorCard.m_agility,
			Player.Get().ActionLeft(),
			Player.Get().m_resources,
			Player.Get().m_clues,
			m_currentAct.m_currentToken,
			m_currentAgenda.m_currentToken);

		m_revealedLocations.ForEach(loc => 
		{
			ui.m_statsInfoText.text += string.Format("{0}：<color=orange>{1}</color>\n", loc.m_cardName, loc.m_clues);
		});

		ui.m_statsInfoText.text += "资产区统计：\n";

		var ally = Player.Get().GetAssetCardInSlot(AssetSlot.Ally) as AllyCard;
		if(ally != null)
		{
			ui.m_statsInfoText.text += string.Format("{0}\n血量：<color=green>{1}</color> 神智：<color=green>{2}</color>\n", ally.m_cardName, ally.m_health, ally.m_sanity);
		}


		var assets = Player.Get().GetAssetAreaCards();
		assets.ForEach((asset) =>
		{
			var logic = asset.GetComponent<PlayerCardLogic>();
			if(logic && logic.HasUseLimit())
			{
				ui.m_statsInfoText.text += string.Format("{0}\n剩余使用次数：<color=green>{1}</color>\n", asset.m_cardName, logic.GetAssetResource());
			}
		});

		ui.m_statsInfoText.text += "威胁区统计：\n";

		var enemies = Player.Get().GetEnemyCards();
		enemies.ForEach(enemy =>
		{
			ui.m_statsInfoText.text += string.Format("{0}血量：<color=red>{1}</color>\n", enemy.m_cardName, enemy.m_health);
		});
	}

	public override int GetChaosTokenEffect(ChaosBag.ChaosTokenType t)
	{
		Assert.IsTrue(false, "Assert failed in GetChaosTokenEffect()!!");
		return -1;
	}

	public override void AfterSkillTestFailed(ChaosBag.ChaosTokenType t)
	{
		
	}

	public override void AdvanceAct()
	{
		
	}

	public override void AdvanceAgenda()
	{
		
	}
}
