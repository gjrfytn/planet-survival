using UnityEngine;
using System.Collections.Generic;

public class BattleController : MonoBehaviour
{
	List<LivingBeing> LBs=new List<LivingBeing>();
	byte Index;

    void OnEnable()
    {
		EventManager.LocalMapEntered+=Activate;
		EventManager.LocalMapLeft+=Deactivate;

    }

    void OnDisable()
    {
		EventManager.LocalMapEntered-=Activate;
		EventManager.LocalMapLeft-=Deactivate;

    }

	void Activate()
	{
		EventManager.LivingBeingEndedTurn+=ProceedBattle;
		LBs=(GameObject.FindWithTag("World").GetComponent<World>().CurrentMap as LocalMap).GetAllLivingBeings();
		Index=0;
		ProceedBattle();
	}

	void Deactivate()
	{
		EventManager.LivingBeingEndedTurn-=ProceedBattle;
	}

	void ProceedBattle()
	{
		LBs.Sort((a,b)=>b.Initiative-a.Initiative);
		if(Index==LBs.Count)
			Index=0;
		LBs[Index++].MakeTurn();
	}
}
