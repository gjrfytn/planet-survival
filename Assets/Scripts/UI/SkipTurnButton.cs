using UnityEngine;
using System.Collections;

public class SkipTurnButton : MonoBehaviour
{
    public void SkipTurn()
    {
        EventManager.OnTurn();
    }
}
