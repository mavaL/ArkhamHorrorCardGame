using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosBag
{
	// https://community.fantasyflightgames.com/topic/235139-chaos-token-symbols/
	public enum ChaosTokenType
	{
		Zero = 0,
		Add_1 = 1,
		Skully,
		ElderSign,
		Tentacle,
		Cultist,
		ElderThing,
		Tablet,
		Substract_1 = -1,
		Substract_2 = -2,
		Substract_3 = -3,
		Substract_4 = -4,
		Substract_5 = -5,
		Substract_6 = -6,
		Substract_7 = -7,
		Substract_8 = -8,
	}

	public List<ChaosTokenType>	m_chaosBag = new List<ChaosTokenType>();

	// Use this for initialization
	public void Init (GameDifficulty d)
	{
		switch(d)
		{
			case GameDifficulty.Normal:
				_InitNormal();
				break;
			default:
				Debug.LogError("Chaos bag not impl!!");
				break;
		}
	}

	private void _InitNormal()
	{
		m_chaosBag.Clear();

		m_chaosBag.Add(ChaosTokenType.Add_1);
		m_chaosBag.Add(ChaosTokenType.Zero);
		m_chaosBag.Add(ChaosTokenType.Zero);
		m_chaosBag.Add(ChaosTokenType.Substract_1);
		m_chaosBag.Add(ChaosTokenType.Substract_1);
		m_chaosBag.Add(ChaosTokenType.Substract_1);
		m_chaosBag.Add(ChaosTokenType.Substract_2);
		m_chaosBag.Add(ChaosTokenType.Substract_2);
		m_chaosBag.Add(ChaosTokenType.Substract_3);
		m_chaosBag.Add(ChaosTokenType.Substract_4);
		m_chaosBag.Add(ChaosTokenType.Skully);
		m_chaosBag.Add(ChaosTokenType.Skully);
		m_chaosBag.Add(ChaosTokenType.Cultist);
		m_chaosBag.Add(ChaosTokenType.Tablet);
		m_chaosBag.Add(ChaosTokenType.Tentacle);
		m_chaosBag.Add(ChaosTokenType.ElderSign);
	}

	public static string GetChaosTokenName(ChaosTokenType t)
	{
		switch (t)
		{
			case ChaosTokenType.Add_1: return "加1";
			case ChaosTokenType.Cultist: return "邪教徒";
			case ChaosTokenType.ElderSign: return "远古印记";
			case ChaosTokenType.ElderThing: return "古神";
			case ChaosTokenType.Skully: return "骷髅头";
			case ChaosTokenType.Substract_1: return "减1";
			case ChaosTokenType.Substract_2: return "减2";
			case ChaosTokenType.Substract_3: return "减3";
			case ChaosTokenType.Substract_4: return "减4";
			case ChaosTokenType.Substract_5: return "减5";
			case ChaosTokenType.Substract_6: return "减6";
			case ChaosTokenType.Substract_7: return "减7";
			case ChaosTokenType.Substract_8: return "减8";
			case ChaosTokenType.Tablet: return "碑盘";
			case ChaosTokenType.Tentacle: return "触手";
			case ChaosTokenType.Zero: return "零";
		}

		return "Assert in GetChaosTokenName()";
	}
	
	public ChaosTokenType GetResult()
	{
		return ChaosTokenType.Cultist;
		return m_chaosBag[Random.Range(0, 16)];
	}
}
