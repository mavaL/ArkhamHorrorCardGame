/********************************************************************
	created:	2018/06/07
	created:	7:6:2018   9:09
	author:		maval
	
	TODO:	1. Multiple copies of Frozen in Fear stack.
			2. Also applies to  card abilities with action designators (Move, Fight, Evade).
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class core_frozen_in_fear : TreacheryLogic
{
	private UnityAction<PlayerAction>	m_actionDone;
	private UnityAction					m_roundEnd;
	private UnityAction<int, Card>		m_afterSkillTest;

	public override void OnReveal()
	{
		Player.Get().AddTreachery(GetComponent<TreacheryCard>());

		m_actionDone = new UnityAction<PlayerAction>(OnActionDone);
		m_roundEnd = new UnityAction(OnRoundEnd);
		m_afterSkillTest = new UnityAction<int, Card>(AfterSkillTest);

		Player.Get().m_actionDoneEvent.AddListener(m_actionDone);
		GameLogic.Get().m_mainGameUI.m_roundEndEvent.AddListener(m_roundEnd);
	}

	public void OnActionDone(PlayerAction action)
	{
		if(action == PlayerAction.Move || action == PlayerAction.Fight || action == PlayerAction.Evade)
		{
			if(!Player.Get().m_actionRecord.Contains(action))
			{
				Player.Get().m_actionUsed += 1;
				GameLogic.Get().OutputGameLog(string.Format("{0}受<魂飞魄散>的效果，多消耗了1点行动\n", Player.Get().m_investigatorCard.m_cardName));
			}
		}
		
	}

	private void Update()
	{
		var ui = GameLogic.Get().m_mainGameUI;

		if (GameLogic.Get().m_currentPhase == TurnPhase.InvestigationPhase)
		{
			if (Player.Get().ActionLeft() < 2)
			{
				if(!Player.Get().m_actionRecord.Contains(PlayerAction.Move))
				{
					ui.m_isActionEnable[PlayerAction.Move] = false;
				}
				if (!Player.Get().m_actionRecord.Contains(PlayerAction.Fight))
				{
					ui.m_isActionEnable[PlayerAction.Fight] = false;
				}
				if (!Player.Get().m_actionRecord.Contains(PlayerAction.Evade))
				{
					ui.m_isActionEnable[PlayerAction.Evade] = false;
				}
			}
			else
			{
				ui.m_isActionEnable[PlayerAction.Move] = true;
				ui.m_isActionEnable[PlayerAction.Fight] = true;
				ui.m_isActionEnable[PlayerAction.Evade] = true;
			}
		}
	}

	private void OnRoundEnd()
	{
		var ui = GameLogic.Get().m_mainGameUI;
		var card = GetComponent<TreacheryCard>();
		ui.m_tempHighlightCard = card.gameObject;

		GameLogic.Get().ShowHighlightCardExclusive(card, false);
		ui.m_choiceMode = MainGame.ConfirmButtonMode.SkillTest;
		ui.BeginSelectCardToSpend(SkillType.Willpower);
		GameLogic.Get().m_afterSkillTest.AddListener(m_afterSkillTest);

		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().blocksRaycasts = true;
		GameObject.Find("CanvasGroup").GetComponent<CanvasGroup>().interactable = true;
	}

	public override void OnSkillTest()
	{
		GameLogic.Get().m_currentScenario.m_skillTest.WillpowerTest(3, null);
	}

	public void AfterSkillTest(int result, Card Target)
	{
		if(result >= 0)
		{
			GameLogic.Get().m_afterSkillTest.RemoveListener(m_afterSkillTest);
			Player.Get().m_actionDoneEvent.RemoveListener(m_actionDone);
			GameLogic.Get().m_mainGameUI.m_roundEndEvent.RemoveListener(m_roundEnd);
			GetComponent<TreacheryCard>().Discard();

		}
		GameLogic.Get().OutputGameLog(string.Format("{0}试图摆脱<魂飞魄散>{1}\n", Player.Get().m_investigatorCard.m_cardName, result<0?"失败":"成功"));
	}
}
