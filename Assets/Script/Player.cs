using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionDoneEvent : UnityEvent<PlayerAction> { }

// Value should correspond with ActionDropdown control
public enum PlayerAction
{
	Move = 1,
	Investigate,
	Fight,
	Evade,
	DrawOneCard,
	GainOneResource,
	PlayCard,
	Skip,
	NonStandardAction1,
	NonStandardAction2,
	NonStandardAction3,
	NonStandardAction4,
	NonStandardAction5,
	NonStandardAction6,
	NonStandardAction7,
	NonStandardAction8,
	NonStandardAction9,
	NonStandardAction10,
	NonStandardAction11,
	NonStandardAction12,
	NonStandardAction13,
	NonStandardAction14,
	NonStandardAction15,
	NonStandardAction16,
	NonStandardAction17,
	NonStandardAction18,
	NonStandardAction19,
	NonStandardAction20,
	ReactiveEvent,
	ReactiveAsset,
	AssignDamage,
	AssignHorror,
	BeatcopCardAction
}

public enum AssetSlot
{
	Accessory = 0,
	Body,
	Ally,
	Hand1,
	Hand2,
	TwoHands,
	Arcane1,
	Arcane2,
	TwoArcanes,
	SlotCount,
	None
}

public class Player
{
	public static Player s_player = null;

	public int					m_currentScenario = -1;
	public InvestigatorCard		m_investigatorCard;
	public GameObject			m_playerToken;
	public LocationCard			m_currentLocation;

	public int					m_health = -1;
	public int					m_sanity = -1;
	public int					m_resources = -1;
	public Faction				m_faction;
	public int					m_clues;
	public int					m_actionUsed = 0;
	public int					m_totalActions = 3;
	public int					m_attackDamage = 1;

	public Stack<PlayerAction>	m_currentAction = new Stack<PlayerAction>();
	public ActionDoneEvent		m_actionDoneEvent = new ActionDoneEvent();
	public List<PlayerAction>	m_actionRecord = new List<PlayerAction>();

	public UnityAction<int>		m_onAssignDamage;
	public int					m_assignDamage;
	public EnemyCard			m_attacker;

	// Hand cards
	private List<PlayerCard>	m_lstPlayerCards = new List<PlayerCard>();
    // Asset cards played
    private List<PlayerCard>    m_lstAssetCards = new List<PlayerCard>();
	// Asset slots
	private List<PlayerCard>	m_lstAssetSlots = new List<PlayerCard>();
	// Engaged enemies
	private List<EnemyCard>		m_lstEnemyCards = new List<EnemyCard>();
	// Engaged treachery
	private List<TreacheryCard> m_lstTreacheryCards = new List<TreacheryCard>();

	public Player()
	{
		for(int i=0; i<(int)AssetSlot.SlotCount; ++i)
		{
			m_lstAssetSlots.Add(null);
		}

		m_resources = 5;
		m_onAssignDamage = new UnityAction<int>(OnConfirmAssignDamage);
	}

	static public Player Get()
	{
		if (s_player == null)
		{
			s_player = new Player();
		}

		return s_player;
	}

    public void InitPlayer(InvestigatorCard card)
    {
        m_investigatorCard = card;
        // 如果有父对象被摧毁，其子对象也会被摧毁
        m_investigatorCard.transform.SetParent(null);
        GameObject.DontDestroyOnLoad(m_investigatorCard);

        m_health = m_investigatorCard.m_health;
        m_sanity = m_investigatorCard.m_sanity;
    }
	
	public void AddHandCard(GameObject go)
	{
		if(go != null)
		{
			PlayerCard pc = go.GetComponent<PlayerCard>();
			pc.m_currentLocation = null;

			m_lstPlayerCards.Add(pc);
			go.SetActive(true);

			var ui = GameLogic.Get().m_mainGameUI;
			ListViewItem item = new ListViewItem();
			item.card = pc;
			ui.m_handCardListView.AddItemsAt(ui.m_handCardListView.GetItemsCount(), item);
		}
	}

	public void RemoveHandCard(PlayerCard card)
	{
		if(card != null)
		{
			if(m_lstAssetCards.Contains(card))
			{
				GameLogic.Get().m_mainGameUI.RemoveCardFromListView(GameLogic.Get().m_mainGameUI.m_assetListView, m_lstAssetCards.IndexOf(card), card);
				m_lstAssetCards.Remove(card);

				if (card.m_slot != AssetSlot.None)
				{
					m_lstAssetSlots[(int)card.m_slot] = null;
				}
			}
			else
			{
				GameLogic.Get().m_mainGameUI.RemoveCardFromListView(GameLogic.Get().m_mainGameUI.m_handCardListView, m_lstPlayerCards.IndexOf(card), card);

				m_lstPlayerCards.Remove(card);
			}
		}
	}

	public List<PlayerCard> GetHandCards()
	{
		return m_lstPlayerCards;
	}

	public List<PlayerCard> GetAssetAreaCards()
	{
		return m_lstAssetCards;
	}

	public List<EnemyCard> GetEnemyCards()
	{
		return m_lstEnemyCards;
	}

	public List<TreacheryCard> GetTreacheryCards()
	{
		return m_lstTreacheryCards;
	}

	public PlayerCard GetAssetCardInSlot(AssetSlot slot)
	{
		return m_lstAssetSlots[(int)slot];
	}

	public bool IsAssetCardInPlay(string cardName)
    {
        for(int i=0; i<m_lstAssetCards.Count; ++i)
        {
            if(m_lstAssetCards[i].m_cardName == cardName)
            {
                return true;
            }
        }
        return false;
    }

	public bool CanPlayAnyHandCard()
	{
		foreach (var card in m_lstPlayerCards)
		{
			if(CanPlayHandCard(card))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanPlayHandCard(PlayerCard card)
	{
		bool bAsset = card.m_lstKeywords.Contains(Card.Keyword.Asset) && m_resources >= card.m_cost;

		bool bCanPlayEvent = true;
		if(card.GetComponent<PlayerCardLogic>())
		{
			bCanPlayEvent = card.GetComponent<PlayerCardLogic>().CanTrigger();
		}

		bool bEvent = card.m_lstKeywords.Contains(Card.Keyword.Event) && 
			card.m_eventTiming == EventTiming.InvestigatePhase && 
			m_resources >= card.m_cost &&
			bCanPlayEvent;

		if(bAsset || bEvent)
		{
			return true;
		}

		return false;
	}

	public bool CanPlayEvent(EventTiming timing)
	{
		if(!GameLogic.Get().m_mainGameUI.m_isActionEnable[PlayerAction.PlayCard])
		{
			return false;
		}

		foreach (var card in m_lstPlayerCards)
		{
			if (card.m_eventTiming == timing && 
				m_resources >= card.m_cost && 
				card.GetComponent<PlayerCardLogic>().CanTrigger())
			{
				return true;
			}
		}
		return false;
	}

	public bool CanTriggerAsset(EventTiming timing)
	{
		foreach (var card in m_lstAssetCards)
		{
			if (card.m_eventTiming == timing &&
				card.GetComponent<PlayerCardLogic>().CanTrigger())
			{
				return true;
			}
		}
		return false;
	}

	public bool ChooseAndDiscardAssetCard()
	{
		// TODO: Asset card impl
		return false;
	}

	public int GetHp()
	{
		return m_health;
	}

	public int GetSan()
	{
		return m_sanity;
	}

	public void HealHealth(int amount)
	{
		m_health += 1;
		UnityEngine.Assertions.Assert.IsTrue(m_health <= m_investigatorCard.m_health, "Assert failed in Player.HealHealth()!!!");
		GameLogic.Get().OutputGameLog(string.Format("{0}恢复了{1}点生命\n", Player.Get().m_investigatorCard.m_cardName, amount));
	}

	public void HealSanity(int amount)
	{
		m_sanity += 1;
		UnityEngine.Assertions.Assert.IsTrue(m_sanity <= m_investigatorCard.m_sanity, "Assert failed in Player.HealHorror()!!!");
		GameLogic.Get().OutputGameLog(string.Format("{0}恢复了{1}点神智\n", Player.Get().m_investigatorCard.m_cardName, amount));
	}

	public void DecreaseHealth(EnemyCard attacker, int amount = 1)
	{
		if(m_lstAssetSlots[(int)AssetSlot.Ally] != null)
		{
			m_currentAction.Push(PlayerAction.AssignDamage);
			m_assignDamage = amount;
			m_attacker = attacker;

			var ui = GameLogic.Get().m_mainGameUI;
			ui.m_actionDropdown.gameObject.SetActive(false);
			ui.m_targetDropdown.gameObject.SetActive(true);
			ui.UpdateTargetDropdown(amount, m_lstAssetSlots[(int)AssetSlot.Ally]);
			ui.m_targetDropdown.onValueChanged.AddListener(m_onAssignDamage);
		}
		else
		{
			m_health -= amount;

			if(attacker)
			{
				GameLogic.Get().m_afterAssignDamageEvent.Invoke(attacker, amount, 0);
			}
		}
	}

	public void DecreaseSanity(int amount = 1)
	{
		if (m_lstAssetSlots[(int)AssetSlot.Ally] != null)
		{
			m_currentAction.Push(PlayerAction.AssignHorror);
			m_assignDamage = amount;

			var ui = GameLogic.Get().m_mainGameUI;
			ui.m_actionDropdown.gameObject.SetActive(false);
			ui.m_targetDropdown.gameObject.SetActive(true);
			ui.UpdateTargetDropdown(amount, m_lstAssetSlots[(int)AssetSlot.Ally]);
			ui.m_targetDropdown.onValueChanged.AddListener(m_onAssignDamage);
		}
		else
		{
			m_sanity -= amount;
		}
	}

    public int HowManySanityIsLost()
    {
        return m_investigatorCard.m_sanity - m_sanity;
    }

	public void ActionDone(PlayerAction action, bool bCostAction = true)
	{
		if(bCostAction)
		{
			m_actionUsed += 1;
		}

		m_actionDoneEvent.Invoke(action);
		m_actionRecord.Add(action);

		if (ActionLeft() == 0)
		{
			Player.Get().m_currentAction.Pop();
			GameLogic.Get().m_mainGameUI.EnterEnemyPhase();
		}
		else
		{
			GameLogic.Get().m_mainGameUI.ResetActionDropdown();
		}
	}

	public void ResetAction()
	{
		m_actionUsed = 0;
		m_actionRecord.Clear();
	}

	public int ActionLeft()
	{
		return m_totalActions - m_actionUsed;
	}

	public void AddEngagedEnemy(EnemyCard enemy)
	{
		enemy.m_engaged = true;
		m_lstEnemyCards.Add(enemy);
		GameLogic.m_lstUnengagedEnemyCards.Remove(enemy);
		
		if(enemy.m_currentLocation)
		{
			enemy.m_currentLocation.m_lstCardsAtHere.Remove(enemy);
			enemy.m_currentLocation = null;
			enemy.gameObject.transform.SetParent(GameLogic.Get().m_mainGameUI.transform.root.parent);
		}

		var ui = GameLogic.Get().m_mainGameUI;
		ListViewItem item = new ListViewItem();
		item.card = enemy;
		ui.m_threatListView.AddItemsAt(ui.m_threatListView.GetItemsCount(), item);

		GameLogic.Get().OutputGameLog(string.Format("{0}与<{1}>发生了交战！\n", m_investigatorCard.m_cardName, enemy.m_cardName));
	}

	public void AddTreachery(TreacheryCard treachery)
	{
		m_lstTreacheryCards.Add(treachery);
		treachery.gameObject.SetActive(true);
	}

	public void RemoveEngagedEnemy(EnemyCard enemy)
	{
		enemy.m_engaged = false;
		GameLogic.Get().m_mainGameUI.RemoveCardFromListView(GameLogic.Get().m_mainGameUI.m_threatListView, m_lstEnemyCards.IndexOf(enemy), enemy);
		m_lstEnemyCards.Remove(enemy);
	}

	public int GetSkillValueByType(SkillType skill)
	{
		switch (skill)
		{
			case SkillType.Agility: return m_investigatorCard.m_agility;
			case SkillType.Combat: return m_investigatorCard.m_combat;
			case SkillType.Intellect: return m_investigatorCard.m_intellect;
			case SkillType.Willpower: return m_investigatorCard.m_willPower;
		}

		return -999;
	}

	private void OnConfirmAssignDamage(int index)
	{
		if (index < 1)
		{
			return;
		}

		GameLogic.Get().m_mainGameUI._OnConfirmAssignDamage(index);
	}

	public void AddAssetCard(PlayerCard card, bool bFromHandCard = true)
	{
		if(bFromHandCard)
		{
			RemoveHandCard(card);

			GameLogic.Get().OutputGameLog(string.Format("{0}打出了资产牌<{1}>\n", m_investigatorCard.m_cardName, card.m_cardName));
		}
		else
		{
			// Remove it from location
			if(card.m_currentLocation)
			{
				card.m_currentLocation.m_lstCardsAtHere.Remove(card);
				card.m_currentLocation = null;
				card.gameObject.transform.SetParent(GameLogic.Get().m_mainGameUI.transform.root.parent);
				card.gameObject.SetActive(false);
			}
		}
		m_lstAssetCards.Add(card);

		// Which slot is this asset in?
		if (card.m_slot != AssetSlot.None)
		{
			// TODO: single hand asset and single slot arcane
			PlayerCard oldSlot = m_lstAssetSlots[(int)card.m_slot];
			if(oldSlot != null)
			{
				// Replace this slot asset
				m_lstAssetCards.Remove(oldSlot);
				oldSlot.Discard(true);

				GameLogic.Get().OutputGameLog(string.Format("{0}被替换下场\n", card.m_cardName));
			}
			m_lstAssetSlots[(int)card.m_slot] = card;
		}

		if(card.GetComponent<PlayerCardLogic>())
		{
			card.GetComponent<PlayerCardLogic>().OnReveal(card);
		}

		//card.gameObject.SetActive(true);

		var ui = GameLogic.Get().m_mainGameUI;
		ListViewItem item = new ListViewItem();
		item.card = card;
		ui.m_assetListView.AddItemsAt(ui.m_assetListView.GetItemsCount(), item);
	}

	public bool IsAnyEnemyToFightWith()
	{
		bool b1 = false;
		for (int i = 0; i < m_currentLocation.m_lstCardsAtHere.Count; ++i)
		{
			Card card = m_currentLocation.m_lstCardsAtHere[i];
			if (card is EnemyCard)
			{
				b1 = true;
				break;
			}
		}

		return b1 || GetEnemyCards().Count > 0;
	}

	public EnemyCard GetEnemyFromEngagedOrLocation(int index)
	{
		if(index < m_lstEnemyCards.Count)
		{
			return m_lstEnemyCards[index];
		}
		else
		{
			int n = m_lstEnemyCards.Count - 1;
			for (int i=0; i<m_currentLocation.m_lstCardsAtHere.Count; ++i)
			{
				if(m_currentLocation.m_lstCardsAtHere[i] is EnemyCard)
				{
					++n;
					if(n == index)
					{
						return m_currentLocation.m_lstCardsAtHere[i] as EnemyCard;
					}
				}
			}

			UnityEngine.Assertions.Assert.IsTrue(false, "Assert failed in Player.GetEnemyFromEngagedOrLocation()!!!");
			return null;
		}
	}
}
