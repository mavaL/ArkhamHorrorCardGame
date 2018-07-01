using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class SkillTestEvents : MonoBehaviour
{
	// For card like <Mind over Matter>..
	public SkillType m_replaceSkillTest { get; set; } = SkillType.None;

	public void WillpowerTest(int value, Card target)
	{
		_GeneralSkillTest(SkillType.Willpower, value, target);
	}

	public void IntellectTest(int value, Card target)
	{
		_GeneralSkillTest(SkillType.Intellect, value, target);
	}

	public void CombatTest(int value, Card target)
	{
		_GeneralSkillTest(SkillType.Combat, value, target);
	}

	public void AgilityTest(int value, Card target)
	{
		_GeneralSkillTest(SkillType.Agility, value, target);
	}

	private void _GeneralSkillTest(SkillType skill, int againstValue, Card target)
	{
		if(m_replaceSkillTest != SkillType.None)
		{
			GameLogic.Get().OutputGameLog(string.Format("{0}替代{1}进行检定\n", Player.Get().GetSkillStringByType(m_replaceSkillTest), Player.Get().GetSkillStringByType(skill)));
			skill = m_replaceSkillTest;
		}

		int result = GameLogic.Get().SkillTest(skill, againstValue, target);

		bool bSucceed = result >= 0;
		Card card = GameLogic.Get().m_mainGameUI.m_tempHighlightCard.GetComponent<Card>();
		card.OnSkillTestResult(result);
	}

	public void AfterSkillTest(int result, ChaosBag.ChaosTokenType chaosToken, Card target)
	{
		StartCoroutine(_AfterSkillTest(result, chaosToken, target));
	}

	private IEnumerator _AfterSkillTest(int result, ChaosBag.ChaosTokenType chaosToken, Card target)
	{
		GameLogic.Get().AfterSkillTest(result>=0, chaosToken);
		// TODO: 协程越来越逻辑耦合了。。。。。。。。应该找个方法去掉协程了。。。。。。。。。
		yield return new WaitUntil(() => Player.Get().GetCurrentAction() != PlayerAction.AssignDamage && Player.Get().GetCurrentAction() != PlayerAction.AssignHorror);

		GameLogic.Get().m_afterSkillTest.Invoke(result, target);
	}
}
