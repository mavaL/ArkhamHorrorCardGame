/********************************************************************
	created:	2018/07/05
	created:	5:7:2018   0:29
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_wendy_adams : InvestigatorLogic
{
	public override int OnApplyElderSignEffect()
	{
		if (Player.Get().IsAssetCardInPlay("温蒂的护身符"))
		{
			// Auto succeed
			GameLogic.Get().OutputGameLog("温蒂佩戴了护身符，检定成功！\n");
			return 999;
		}
		else
		{
			return 0;
		}
	}
}
