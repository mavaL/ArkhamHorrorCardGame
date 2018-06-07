using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class SkillTestEvents : MonoBehaviour
{
	public void WillpowerTest(int value)
	{
		_GeneralSkillTest(SkillType.Willpower, value);
	}

	public void IntellectTest(int value)
	{
		_GeneralSkillTest(SkillType.Intellect, value);
	}

	public void CombatTest(int value)
	{
		_GeneralSkillTest(SkillType.Combat, value);
	}

	public void AgilityTest(int value)
	{
		_GeneralSkillTest(SkillType.Agility, value);
	}

	private void _GeneralSkillTest(SkillType skill, int againstValue)
	{
		ChaosBag.ChaosTokenType chaosToken;
		int result = GameLogic.Get().SkillTest(skill, againstValue, out chaosToken);

		bool bSucceed = result >= 0;
		Card card = GameLogic.Get().m_mainGameUI.m_tempHighlightCard.GetComponent<Card>();
		card.OnSkillTestResult(result);

		GameLogic.Get().AfterSkillTest(bSucceed, chaosToken);
	}

	public void SkillTestFailedWithDamage(int result)
	{
		if(result < 0)
		{
			Player.Get().DecreaseHealth(-result);
			GameLogic.Get().OutputGameLog(string.Format("{0}因为技能检定失败受到了{1}点伤害！\n", Player.Get().m_investigatorCard.m_cardName, -result));
		}
	}

	public void SkillTestFailedWithHorror(int result)
	{
		if (result < 0)
		{
			Player.Get().DecreaseSanity(-result);
			GameLogic.Get().OutputGameLog(string.Format("{0}因为技能检定失败受到了{1}点恐怖！\n", Player.Get().m_investigatorCard.m_cardName, -result));
		}
	}
}
