using UnityEngine;
using System.Collections;

public class BattleController : MonoBehaviour
{
    byte CreaturesPending;

    void OnEnable()
    {
        EventManager.PlayerMadeTurn += CreaturesTurn;
    }

    void OnDisable()
    {
        EventManager.PlayerMadeTurn -= CreaturesTurn;
    }

    void CreaturesTurn()//C# 6.0
    {
        EventManager.CreatureStartedTurn += CreatureDoingTurn;
        EventManager.CreatureEndedTurn += CreatureMadeTurn;
    }

    void CreatureDoingTurn()//C# 6.0
    {
        CreaturesPending++;
    }

    void CreatureMadeTurn()
    {
        CreaturesPending--;

        if (CreaturesPending == 0)
        {
            EventManager.CreatureStartedTurn -= CreatureDoingTurn;
            EventManager.CreatureEndedTurn -= CreatureMadeTurn;
            EventManager.OnTurn();
        }
    }
}
