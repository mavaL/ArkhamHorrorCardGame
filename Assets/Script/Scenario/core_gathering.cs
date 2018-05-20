using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
		switch(t)
		{
			case ChaosBag.ChaosTokenType.Skully:

				break;
		}
		return -1;
	}
}
