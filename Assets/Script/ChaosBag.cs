using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosBag : MonoBehaviour
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
	void Start ()
	{
		switch(GameLogic.Get().m_difficulty)
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
	
	public ChaosTokenType GetResult()
	{
		return m_chaosBag[Random.Range(0, 16)];
	}
}
