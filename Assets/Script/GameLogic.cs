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

    public Player	m_player = new Player();
 

    static public GameLogic Get()
    {
        if(s_gameLogic == null)
        {
            s_gameLogic = new GameLogic();
        }

        return s_gameLogic;
    }

	public static void Swap<T>(ref T a, ref T b)
	{
		T t = a;
		a = b;
		b = t;
	}

	void Start()
	{
		UnityEngine.Random.InitState((int)System.DateTime.Now.ToUniversalTime().ToBinary());
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

		return card;
	}
}
