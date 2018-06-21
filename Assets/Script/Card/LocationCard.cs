using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LocationCard : Card
{
	public int					m_clues = 0;
	public int					m_shroud = 0;
	public List<LocationCard>	m_lstDestinations;

	[System.NonSerialized]
	public SkillTestEvent		m_onLocationInvestigate = new SkillTestEvent();
	[System.NonSerialized]
	public List<Card>			m_lstCardsAtHere = new List<Card>();
	public bool					m_isVisit { get; set; } = false;

	#region
	/// fields for pathfinding
	public LocationCard			m_BFS_parent { get; set; }
	public bool					m_BFS_checked { get; set; }
	#endregion


	[System.Serializable]
	public class LocationCallback : UnityEvent<LocationCard> { }
	public LocationCallback		m_enterLocationCallback;
	public LocationCallback		m_locationAbilityCallback;

	public void EnterLocation()
	{
		string log;

		if(!m_isVisit)
		{
			m_canFlip = true;
			FlipCard();
			m_isVisit = true;

			GameLogic.Get().m_currentScenario.m_revealedLocations.Add(this);
			log = string.Format("{0}发现了{1}\n", Player.Get().m_investigatorCard.m_cardName, m_cardName);
		}
		else
		{
			log = string.Format("{0}移动到了{1}\n", Player.Get().m_investigatorCard.m_cardName, m_cardName);
		}
		GameLogic.Get().OutputGameLog(log);

		m_enterLocationCallback.Invoke(this);

		// Player token
		Player.Get().m_playerToken.transform.SetParent(gameObject.transform);

		var rt = Player.Get().m_playerToken.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.sizeDelta.x / 2, rt.sizeDelta.y / 2);
		rt.localScale = new Vector3(1, 1, 1);

		// Is this location has any ability
		bool bLocationHasAbility = m_locationAbilityCallback.GetPersistentEventCount() > 0;
		GameLogic.Get().m_mainGameUI.m_useLocationAbilityBtn.gameObject.SetActive(bLocationHasAbility);

		// Are there any enemies at this location
		for(int i=0; i<m_lstCardsAtHere.Count; ++i)
		{
			Card card = m_lstCardsAtHere[i];
			if(card is EnemyCard)
			{
				Player.Get().AddEngagedEnemy(card as EnemyCard);
			}
		}
	}

	public override void OnSkillTest()
	{
		GameLogic.Get().m_currentScenario.m_skillTest.IntellectTest(m_shroud);
	}

	public override void OnSkillTestResult(int result)
	{
		//result = 99;
		if (result >= 0)
		{
			// Succeed!
			Player.Get().m_clues += 1;
			m_clues -= 1;
			GameLogic.Get().OutputGameLog("调查成功！\n");
		}
		else
		{
			// Failed..
			GameLogic.Get().OutputGameLog("调查失败！\n");
		}

		m_onLocationInvestigate.Invoke(result);

		Player.Get().ActionDone(PlayerAction.Investigate);
	}

	public override void Discard(bool bFromAssetArea = false)
	{
		gameObject.SetActive(false);

		foreach(var card in m_lstCardsAtHere)
		{
			card.Discard();
		}
	}
}
