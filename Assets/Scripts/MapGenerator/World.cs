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
	private List<Map> LocalMaps = new List<Map> ();
	private WorldGenerator Generator; //readonly?

	//TODO Можно ли использовать такую форму?
	//public Map GlobalMap{get;private set;}
	//public Map[] LocalMaps{get;private set;}

	private WorldVisualiser Visualiser; //Временно

	void Start ()
	{
		Generator = GetComponent<WorldGenerator> ();
		Visualiser = GetComponent<WorldVisualiser>(); //Временно

		// Это всё временно, как пример. На самом деле карта должна создаваться только при начале новой игры, иначе загружаться из сохранения.
		//--
		GlobalMap = new Map (Generator.GlobalMapSize);

		Generator.CreateHeightmap (GlobalMap.MatrixHeight, Generator.LandscapeRoughness);
		Generator.CreateHeightmap (GlobalMap.MatrixForest, Generator.ForestRoughness);
		Generator.CreateRivers (GlobalMap.MatrixHeight, GlobalMap.MatrixRiver);

		CurrentMap = GlobalMap;

		//Visualiser.RenderNewMap(CurrentMap);
		Visualiser.RenderVisibleHexes(new Vector2(5,5),GameObject.FindWithTag("Player").GetComponent<PlayerData>().ViewDistance,CurrentMap);
		//--
	}

	/// <summary>
	/// Создаёт локальную карту.
	/// </summary>
	/// <returns>Индекс новой карты.</returns>
	public uint CreateLocalMap ()
	{
		Map map = new Map (Generator.LocalMapSize);

		Generator.CreateHeightmap (map.MatrixHeight, Generator.LandscapeRoughness);
		Generator.CreateHeightmap (map.MatrixForest, Generator.ForestRoughness);
		Generator.CreateRivers (map.MatrixHeight, map.MatrixRiver);

		LocalMaps.Add (map);
		return (uint)LocalMaps.Count - 1;
	}
}
