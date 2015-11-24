using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Это класс карты, хранит в себе слои в виде матриц
public class Map
{
	public float[,] MatrixHeight;
	public float[,] MatrixForest;
	public bool[,] MatrixRiver;
	
	public Map (ushort size)
	{
		MatrixHeight = new float[size, size];
		MatrixForest = new float[size, size];
		MatrixRiver = new bool[size, size];
	}
}

public class World : MonoBehaviour
{
	public Map CurrentMap; // Текущая карта, отображаемая на экране. Возможно должна храниться не здесь.

	private Map GlobalMap;

	private Map[,] LocalMaps;
	//private List<Map> LocalMaps = new List<Map> ();
	private WorldGenerator Generator; //readonly?

	//TODO Можно ли использовать такую форму?
	//public Map GlobalMap{get;private set;}
	//public Map[] LocalMaps{get;private set;}

	private WorldVisualiser Visualiser; //Временно

	private Vector2 GlobalMapCoords;

	void Start ()
	{
		Generator = GetComponent<WorldGenerator> ();
		Visualiser = GetComponent<WorldVisualiser>(); //Временно

		// Это всё временно, как пример. На самом деле карта должна создаваться только при начале новой игры, иначе загружаться из сохранения.
		//--
		GlobalMap = new Map (Generator.GlobalMapSize);
		LocalMaps=new Map[Generator.GlobalMapSize,Generator.GlobalMapSize];

		Generator.CreateHeightmap (GlobalMap.MatrixHeight, Generator.LandscapeRoughness);
		Generator.CreateHeightmap (GlobalMap.MatrixForest, Generator.ForestRoughness);
		Generator.CreateRivers (GlobalMap.MatrixHeight, GlobalMap.MatrixRiver);

		CurrentMap = GlobalMap;

		//Visualiser.RenderNewMap(CurrentMap);
		Visualiser.RenderVisibleHexes(GameObject.FindWithTag("Player").GetComponent<PlayerData>().MapCoords,GameObject.FindWithTag("Player").GetComponent<PlayerData>().ViewDistance,CurrentMap);
		//--
	}

	public void SwitchMap()
	{
		if(CurrentMap==GlobalMap)
			GotoLocalMap();
		else
			GotoGlobalMap();
	}

	void GotoLocalMap()
		{
		//TEMP
		GameObject player=GameObject.FindWithTag("Player");
		Vector2 mapCoords=player.GetComponent<PlayerData>().MapCoords;
		//

			if(LocalMaps[(int)mapCoords.y,(int)mapCoords.x]==null)
				CreateLocalMap(mapCoords);
			CurrentMap=LocalMaps[(int)mapCoords.y,(int)mapCoords.x];
			GlobalMapCoords=mapCoords;

		//TEMP
		player.GetComponent<PlayerData>().MapCoords.x=CurrentMap.MatrixHeight.GetLength(0)/2;
		player.GetComponent<PlayerData>().MapCoords.y=CurrentMap.MatrixHeight.GetLength(0)/2;
		//
	}

	void GotoGlobalMap()
	{
		CurrentMap=GlobalMap;
		GameObject.FindWithTag("Player").GetComponent<PlayerData>().MapCoords=GlobalMapCoords; //TODO Поиск или объект?
	}

	/// <summary>
	/// Создаёт локальную карту.
	/// </summary>
	/// <param name="coords">Координаты новой карты на глобальной.</param>
	void CreateLocalMap (Vector2 mapCoords)
	{
		Map map = new Map (Generator.LocalMapSize);

		Generator.CreateHeightmap (map.MatrixHeight, Generator.LandscapeRoughness);
		Generator.CreateHeightmap (map.MatrixForest, Generator.ForestRoughness);
		Generator.CreateRivers (map.MatrixHeight, map.MatrixRiver);

		LocalMaps[(int)mapCoords.y,(int)mapCoords.x]=map;
		//LocalMaps.Add (map);
		//return (uint)LocalMaps.Count - 1;
	}
}
