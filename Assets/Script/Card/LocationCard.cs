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
	public List<Card>			m_lstCardsAtHere = new List<Card>();
	[System.NonSerialized]
	public bool					m_isVisit = false;

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

		bool bLocationHasAbility = m_locationAbilityCallback.GetPersistentEventCount() > 0;
		GameLogic.Get().m_mainGameUI.m_useLocationAbilityBtn.gameObject.SetActive(bLocationHasAbility);
	}

	public override void OnSkillTest()
	{
		GameLogic.Get().OutputGameLog(string.Format("{0}调查了{1}\n", Player.Get().m_investigatorCard.m_cardName, m_cardName));
		GameLogic.Get().m_currentScenario.m_skillTest.IntellectTest(m_shroud);
	}

	public override void OnSkillTestResult(int result)
	{
		if (/*result >= 0*/true)
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
	}
}
