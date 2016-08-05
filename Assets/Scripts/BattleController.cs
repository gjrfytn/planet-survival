using UnityEngine;
using System.Collections.Generic;

public class BattleController : MonoBehaviour
{
    List<LivingBeing> LBs = new List<LivingBeing>();
    byte Index;

    void OnEnable()
    {
        EventManager.LocalMapEntered += Activate;
        EventManager.LocalMapLeft += Deactivate;
        EventManager.CreatureDied += ExcludeLivingBeing;
    }

    void OnDisable()
    {
        EventManager.LocalMapEntered -= Activate;
        EventManager.LocalMapLeft -= Deactivate;
        EventManager.CreatureDied -= ExcludeLivingBeing;
    }

    void Activate()
    {
        EventManager.LivingBeingEndedTurn += ProceedBattle;
        LBs = (GameObject.FindWithTag("World/World").GetComponent<World>().CurrentMap as LocalMap).GetAllLivingBeings();
        Index = 0;
        ProceedBattle();
    }

    void Deactivate()
    {
        EventManager.LivingBeingEndedTurn -= ProceedBattle;
    }

    void ProceedBattle()
    {
        System.Linq.Enumerable.OrderByDescending(LBs, lb => lb.Initiative); //List<T>.Sort - нестабильный
        if (Index >= LBs.Count)
            Index = 0;
        LBs[Index++].MakeTurn();
    }

    void ExcludeLivingBeing(LivingBeing lb)
    {
        LBs.Remove(lb);
    }
}
