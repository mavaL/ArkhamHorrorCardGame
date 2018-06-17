/********************************************************************
	created:	2018/06/17
	created:	17:6:2018   20:35
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_guard_dog : PlayerCardLogic
{
	private UnityAction<EnemyCard, int, int>	m_afterAssignDamage;

	public override void OnReveal(Card card)
	{
		m_afterAssignDamage = new UnityAction<EnemyCard, int, int>(AfterAssignDamage);

		GameLogic.Get().m_afterAssignDamageEvent.AddListener(m_afterAssignDamage);
	}

	private void AfterAssignDamage(EnemyCard attacker, int investigatorDamage, int allyDamage)
	{
		GameLogic.Get().m_afterAssignDamageEvent.RemoveListener(m_afterAssignDamage);

		UnityEngine.Assertions.Assert.IsNotNull(attacker, "Assert failed in core_guard_dog.AfterAssignDamage()!!!");

		if(allyDamage != 0)
		{
			GameLogic.Get().OutputGameLog(string.Format("<看门狗>对<{0}>发动反击\n", attacker.m_cardName));

			attacker.DecreaseHealth(1);
		}
	}
}
