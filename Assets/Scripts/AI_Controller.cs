using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Controller : MonoBehaviour 
{
	List<GameObject> Creatures=new List<GameObject>();

	public void AddAI(GameObject creature)
	{
		Creatures.Add(creature);
	}

	public void DeleteAll()
	{
		Creatures.ForEach(c=>Destroy(c));
		Creatures.Clear();
	}

	public void MakeTurn()
	{
		for(ushort i=0;i<Creatures.Count;++i)
			if(Creatures[i].GetComponent<Creature>().Health<=0)
			{
				Destroy(Creatures[i]);
				Creatures.RemoveAt(i);
				i--;
			}
		Creatures.ForEach(c=>c.GetComponent<Creature>().MakeTurn());
	}
}
