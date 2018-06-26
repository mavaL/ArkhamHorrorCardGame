using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class SkillTestEvents : MonoBehaviour
{
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
		ChaosBag.ChaosTokenType chaosToken;
		int result = GameLogic.Get().SkillTest(skill, againstValue, target, out chaosToken);

		bool bSucceed = result >= 0;
		Card card = GameLogic.Get().m_mainGameUI.m_tempHighlightCard.GetComponent<Card>();
		card.OnSkillTestResult(result);

		GameLogic.Get().AfterSkillTest(bSucceed, chaosToken);
	}

	public void SkillTestFailedWithDamage(int result)
	{
		if(result < 0)
		{
			Player.Get().DecreaseHealth(null, -result);
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

	public void OnTreacheryReveal_Ancient_Evils()
	{
		var agenda = GameLogic.Get().m_currentScenario.m_currentAgenda;

		if (agenda.AddDoom())
		{
			GameLogic.Get().OutputGameLog("恶兆已集满，触发事件！\n");
			GameLogic.Get().ShowHighlightCardExclusive(agenda, true);
			GameLogic.Get().m_mainGameUI.m_confirmAgendaResultBtn.gameObject.SetActive(true);
		}
	}

	public void OnSkillTestResult_Crypt_Chill(int result)
	{
		if(result < 0)
		{
			if(!Player.Get().ChooseAndDiscardAssetCard())
			{
				Player.Get().DecreaseHealth(null, 2);
				GameLogic.Get().OutputGameLog(string.Format("{0}结算<地穴恶寒>，因为没有在场资产牌，受到了2点伤害！\n", Player.Get().m_investigatorCard.m_cardName));
			}
		}
	}

	public void OnSkillTestResult_Vicious_Blow(int result, GameObject go)
	{
		if(result >= 0)
		{
			var enemy = go.GetComponent<EnemyCard>();
			enemy.DecreaseHealth(1);
			GameLogic.Get().OutputGameLog(string.Format("<{0}>因为<残忍打击>而多受了1点伤害！\n", enemy.m_cardName));
		}
	}
}
