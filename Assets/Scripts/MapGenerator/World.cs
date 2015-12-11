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
	public Map CurrentMap; // Текущая карта, отображаемая на экране.

	Map GlobalMap;

	Map[,] LocalMaps;

	WorldGenerator Generator; //readonly?

	WorldVisualiser Visualiser; //Временно

	Vector2 GlobalMapCoords;

	GameObject Player;

	void Start ()
	{
		Generator = GetComponent<WorldGenerator> ();
		Visualiser = GetComponent<WorldVisualiser>(); //Временно

		// Это всё временно, как пример. На самом деле карта должна создаваться только при начале новой игры, иначе загружаться из сохранения.
		//--
		GlobalMap = new Map (Generator.GlobalMapChunkSize);
		LocalMaps = new Map[Generator.GlobalMapChunkSize,Generator.GlobalMapChunkSize];

		Generator.CreateHeightmap (GlobalMap.MatrixHeight, Generator.LandscapeRoughness,Random.value,Random.value,Random.value,Random.value);
		Generator.CreateHeightmap (GlobalMap.MatrixForest, Generator.ForestRoughness,Random.value,Random.value,Random.value,Random.value);
		Generator.CreateRivers (GlobalMap.MatrixHeight, GlobalMap.MatrixRiver);

		CurrentMap = GlobalMap;

		//Visualiser.RenderWholeMap(CurrentMap);
		Player=GameObject.FindWithTag("Player");
		Visualiser.RenderVisibleHexes(Player.GetComponent<PlayerData>().MapCoords,Player.GetComponent<PlayerData>().ViewDistance,CurrentMap);
		//--
	}

	public void OnGotoHex()
	{
		if(CurrentMap==GlobalMap)
		Visualiser.RenderVisibleHexes(Player.GetComponent<PlayerData>().MapCoords,Player.GetComponent<PlayerData>().ViewDistance,CurrentMap);
	}

	/// <summary>
	/// Переключение карт.
	/// </summary>
	public void SwitchMap()
	{
		if(CurrentMap==GlobalMap)
			GotoLocalMap();
		else
			GotoGlobalMap();
	}

	/// <summary>
	/// Переход на локальную карту.
	/// </summary>
	void GotoLocalMap()
	{
		Vector2 mapCoords=Player.GetComponent<PlayerData>().MapCoords;
			if(LocalMaps[(int)mapCoords.y,(int)mapCoords.x]==null)
				CreateLocalMap(mapCoords,GlobalMap.MatrixHeight[(int)mapCoords.y,(int)mapCoords.x],GlobalMap.MatrixForest[(int)mapCoords.y,(int)mapCoords.x]);
			CurrentMap=LocalMaps[(int)mapCoords.y,(int)mapCoords.x];
			GlobalMapCoords=mapCoords;

		//TEMP
		Player.GetComponent<PlayerData>().MapCoords.x=CurrentMap.MatrixHeight.GetLength(0)/2;
		Player.GetComponent<PlayerData>().MapCoords.y=CurrentMap.MatrixHeight.GetLength(0)/2;
		//
		Visualiser.RenderWholeMap(CurrentMap);
	}

	/// <summary>
	/// Переход на глобальную карту.
	/// </summary>
	void GotoGlobalMap()
	{
		CurrentMap=GlobalMap;
		Player.GetComponent<PlayerData>().MapCoords=GlobalMapCoords;
		Visualiser.RenderVisibleHexes(Player.GetComponent<PlayerData>().MapCoords,Player.GetComponent<PlayerData>().ViewDistance,CurrentMap);
	}

	/// <summary>
	/// Создаёт локальную карту.
	/// </summary>
	/// <param name="coords">Координаты новой карты на глобальной.</param>
	void CreateLocalMap (Vector2 mapCoords,float height,float forest)
	{
		Map map = new Map (Generator.LocalMapSize);

		Generator.CreateHeightmap (map.MatrixHeight, Generator.LandscapeRoughness,height,height,height,height);
		Generator.CreateHeightmap (map.MatrixForest, Generator.ForestRoughness,forest,forest,forest,forest);
		Generator.CreateRivers (map.MatrixHeight, map.MatrixRiver);

		LocalMaps[(int)mapCoords.y,(int)mapCoords.x]=map;
		//LocalMaps.Add (map);
		//return (uint)LocalMaps.Count - 1;
	}
}
