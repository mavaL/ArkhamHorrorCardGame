﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InvestigatorCard : Card
{
	public int	m_willPower;
	public int	m_intellect;
	public int	m_combat;
	public int	m_agility;

	public int	m_health;
	public int	m_sanity;

    [System.Serializable]
    public class ElderSignAbility : SerializableCallback<int> { }
    public ElderSignAbility m_investigatorAbility;

    [System.Serializable]
    public class AfterElderSign : SerializableCallback<bool, int> { }
    public AfterElderSign   m_afterElderSignEvent;
}
