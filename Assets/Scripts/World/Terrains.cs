using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Terrains : MonoBehaviour 
{
	[System.Serializable]
	public class TerrainProperties
	{
		public TerrainType Terrain;
		public ushort TravelTime;

	}

	public TerrainProperties[] TerrainsList;

	void Awake () 
	{
		for(byte i=0;i<TerrainsList.Length;++i)
			for(byte j=0;j<TerrainsList.Length;++j)
				if(TerrainsList[i].Terrain==TerrainsList[j].Terrain&&i!=j)
					throw new System.Exception("Duplicatated terrain types in TerrainsList.");
	}
}
