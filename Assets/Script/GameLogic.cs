using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum GameDifficulty
{
    Easy,
    Normal,
    Hard,
    Expert
}

public enum Faction
{
	Guardian,
	Survivor,
	Mystic,
	Rogue,
	Seeker
}



public class GameLogic
{
    public static GameLogic s_gameLogic = null;

	public GameLogic()
	{
		UnityEngine.Random.InitState((int)System.DateTime.Now.ToUniversalTime().ToBinary());

		m_core_scenarios[0] = new core_gathering();
	}

    static public GameLogic Get()
    {
        if(s_gameLogic == null)
        {
            s_gameLogic = new GameLogic();
        }

        return s_gameLogic;
    }

	public scenario_base[]	m_core_scenarios = new scenario_base[3];

	public static void Swap<T>(ref T a, ref T b)
	{
		T t = a;
		a = b;
		b = t;
	}

	public static void DockCard(GameObject go, GameObject parent)
	{
		go.transform.SetParent(parent.transform);
		go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
		go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
	}

	public static void DockPlayerToken(GameObject go, GameObject parent)
	{
		go.transform.SetParent(parent.transform);

		var rt = go.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.sizeDelta.x/2, rt.sizeDelta.y/2);
		rt.localScale = new Vector3(1, 1, 1);
	}


	public static void Shuffle(List<GameObject> cards)
	{
		for(int i=0; i<cards.Count; ++i)
		{
			int newPos = Random.Range(0, cards.Count-1);
			var t = cards[i];
			cards[i] = cards[newPos];
			cards[newPos] = t;
		}
	}

	public static GameObject DrawCard(List<GameObject> cards)
	{
		if(cards.Count == 0)
		{
			return null;
		}

		var card = cards[0];
		cards.RemoveAt(0);
		card.SetActive(true);

		return card;
	}

	public void StartScenario()
	{
		m_core_scenarios[0].StartScenario();
	}

	public void Update()
	{
		m_core_scenarios[0].ShowPlayInfo();
	}
}
