using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

 [System.Serializable]
 public class SkillIconDictionary : SerializableDictionary<PlayerCard.SkillIcon, int> {}


public class PlayerCard : Card
{
	public enum SkillIcon
	{
		Willpower,
		Intellect,
		Combat,
		Agility
	}

	public SkillIconDictionary m_skillIcons;
}
