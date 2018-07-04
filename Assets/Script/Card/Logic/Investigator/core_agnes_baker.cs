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

public class core_agnes_baker : InvestigatorLogic
{
	public override int OnApplyElderSignEffect()
	{
		return Player.Get().HowManySanityIsLost();
	}
}
