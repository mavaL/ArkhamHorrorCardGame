/********************************************************************
	created:	2018/06/18
	created:	18:6:2018   22:09
	author:		maval
	
	TODO:		1. Player may have 2 guns at hand..	should offer choice.
				2. This card not support two-hand firearms now, is there any that kind of weapon?
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_extra_ammunition : PlayerCardLogic
{
	public override void OnReveal(Card card)
	{
		var firearm = Player.Get().GetAssetCardInSlot(AssetSlot.Hand1);
		if(!firearm.IsKeywordContain(Card.Keyword.Firearm))
		{
			firearm = Player.Get().GetAssetCardInSlot(AssetSlot.Hand2);
			UnityEngine.Assertions.Assert.IsTrue(firearm.IsKeywordContain(Card.Keyword.Firearm), "Assert failed in core_extra_ammunition.OnReveal()!!!");
		}

		firearm.GetComponent<PlayerCardLogic>().AddAssetResource(3);

		GameLogic.Get().OutputGameLog(string.Format("{0}打出对<{1}><额外弹药>，花费{2}资源，增加了3子弹\n", Player.Get().m_investigatorCard.m_cardName, firearm.m_cardName, GetComponent<PlayerCard>().m_cost));

		Player.Get().m_currentAction = PlayerAction.None;
	}

	public override bool CanTrigger()
	{
		return Card.HowManyPlayerCardContainTheKeyword(Player.Get().GetAssetAreaCards(), Card.Keyword.Firearm) > 0;
	}
}
