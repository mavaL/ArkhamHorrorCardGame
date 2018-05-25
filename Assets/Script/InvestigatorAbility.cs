using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InvestigatorAbility : MonoBehaviour
{

    public int OnElderSignAbility_Roland()
    {
        return Player.Get().m_currentLocation.m_clues;
    }

    public int OnElderSignAbility_Wendy()
    {
        if(Player.Get().IsAssetCardInPlay("温蒂的护身符"))
        {
            // Auto succeed
            GameLogic.Get().OutputGameLog("温蒂佩戴了护身符，检定成功！\n");
            return 999;
        }
        else
        {
            return 0;
        }
    }

    public int OnElderSignAbility_Agnes()
    {
        return Player.Get().HowManySanityIsLost();
    }

    public int OnElderSignAbility_OToole()
    {
        return 2;
    }

    public int AfterElderSignAbility_OToole(bool bSucceed)
    {
        if(bSucceed)
        {
            Player.Get().m_resources += 2;
            GameLogic.Get().OutputGameLog("奥图尔触发能力获得了2资源！\n");
        }
        return 0;
    }

    public int OnElderSignAbility_Daisy()
    {
        return Player.Get().m_currentLocation.m_clues;
    }
}
