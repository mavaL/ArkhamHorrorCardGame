using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: lua脚本化
public class Role
{
    public string   m_name;
    public int      m_currentScenario = -1;
	public string	m_scenarioName;
}

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

    private Role _role = new Role();
    public Role PlayerRole
    {
        get { return _role; }
        set { _role = value; }
    }
 

    static public GameLogic Get()
    {
        if(s_gameLogic == null)
        {
            s_gameLogic = new GameLogic();
        }

        return s_gameLogic;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
