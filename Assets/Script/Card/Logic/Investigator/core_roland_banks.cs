/********************************************************************
	created:	2018/07/04
	created:	4:7:2018   20:10
	author:		maval
	
	purpose:	
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_roland_banks : InvestigatorLogic
{
	public override int OnApplyElderSignEffect()
	{
		return Player.Get().m_currentLocation.m_clues;
	}
}
