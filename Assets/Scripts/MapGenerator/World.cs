﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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
	public ushort GlobalMapChunkSize; //Должен быть 2 в n-ой степени
	public ushort LocalMapSize; //Должен быть 2 в n-ой степени

	public Map CurrentMap; // Карта, на которой находится игрок.

	//const byte CashedChunksSize=3;

	Map[,] CashedGlobalMapChunks = new Map[3, 3];
	Map[,] LocalMaps;
	WorldGenerator Generator; //readonly?

	WorldVisualiser Visualiser; //Временно

	Vector2 GlobalMapCoords;
	GameObject Player;
	int ChunkX, ChunkY;
	const string ChunksDirectoryName = "chunks";
	string ChunksDirectoryPath;

	void Awake ()
	{
		Debug.Assert (Mathf.IsPowerOfTwo (GlobalMapChunkSize));
		Debug.Assert (Mathf.IsPowerOfTwo (LocalMapSize));

		GlobalMapChunkSize++; //TODO !!!
		LocalMapSize++; //TODO !!!
	}

	void Start ()
	{
		ChunksDirectoryPath = Path.Combine (Application.dataPath, ChunksDirectoryName);

		if (Directory.Exists (ChunksDirectoryPath))
			Directory.Delete (ChunksDirectoryPath, true);

		Generator = GetComponent<WorldGenerator> ();
		Visualiser = GetComponent<WorldVisualiser> (); //Временно

		// Это всё временно, как пример. На самом деле карта должна создаваться только при начале новой игры, иначе загружаться из сохранения.
		//--
		ChunkX = ChunkY = 0;

		CashedGlobalMapChunks [1, 1] = new Map (GlobalMapChunkSize);
		Generator.CreateHeightmap (CashedGlobalMapChunks [1, 1].MatrixHeight, Generator.LandscapeRoughness, Random.value, Random.value, Random.value, Random.value);
		Generator.CreateHeightmap (CashedGlobalMapChunks [1, 1].MatrixForest, Generator.ForestRoughness, Random.value, Random.value, Random.value, Random.value);
		Generator.CreateRivers (CashedGlobalMapChunks [1, 1].MatrixHeight, CashedGlobalMapChunks [1, 1].MatrixRiver);

		CashedGlobalMapChunks [0, 0] = new Map (GlobalMapChunkSize);
		Generator.CreateHeightmap (CashedGlobalMapChunks [0, 0].MatrixHeight, Generator.LandscapeRoughness, Random.value, CashedGlobalMapChunks [1, 1].MatrixHeight [0, 0], Random.value, Random.value);
		Generator.CreateHeightmap (CashedGlobalMapChunks [0, 0].MatrixForest, Generator.ForestRoughness, Random.value, CashedGlobalMapChunks [1, 1].MatrixForest [0, 0], Random.value, Random.value);
		Generator.CreateRivers (CashedGlobalMapChunks [0, 0].MatrixHeight, CashedGlobalMapChunks [0, 0].MatrixRiver);

		CashedGlobalMapChunks [0, 1] = new Map (GlobalMapChunkSize);
		Generator.CreateHeightmap (CashedGlobalMapChunks [0, 1].MatrixHeight, Generator.LandscapeRoughness, CashedGlobalMapChunks [1, 1].MatrixHeight [0, 0], CashedGlobalMapChunks [1, 1].MatrixHeight [0, GlobalMapChunkSize - 1], CashedGlobalMapChunks [0, 0].MatrixHeight [0, GlobalMapChunkSize - 1], Random.value);
		Generator.CreateHeightmap (CashedGlobalMapChunks [0, 1].MatrixForest, Generator.ForestRoughness, CashedGlobalMapChunks [1, 1].MatrixForest [0, 0], CashedGlobalMapChunks [1, 1].MatrixForest [0, GlobalMapChunkSize - 1], CashedGlobalMapChunks [0, 0].MatrixForest [0, GlobalMapChunkSize - 1], Random.value);
		Generator.CreateRivers (CashedGlobalMapChunks [0, 1].MatrixHeight, CashedGlobalMapChunks [0, 1].MatrixRiver);

		CashedGlobalMapChunks [0, 2] = new Map (GlobalMapChunkSize);
		Generator.CreateHeightmap (CashedGlobalMapChunks [0, 2].MatrixHeight, Generator.LandscapeRoughness, CashedGlobalMapChunks [1, 1].MatrixHeight [0, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks [0, 1].MatrixHeight [0, GlobalMapChunkSize - 1], Random.value);
		Generator.CreateHeightmap (CashedGlobalMapChunks [0, 2].MatrixForest, Generator.ForestRoughness, CashedGlobalMapChunks [1, 1].MatrixForest [0, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks [0, 1].MatrixForest [0, GlobalMapChunkSize - 1], Random.value);
		Generator.CreateRivers (CashedGlobalMapChunks [0, 2].MatrixHeight, CashedGlobalMapChunks [0, 2].MatrixRiver);

		CashedGlobalMapChunks [1, 0] = new Map (GlobalMapChunkSize);
		Generator.CreateHeightmap (CashedGlobalMapChunks [1, 0].MatrixHeight, Generator.LandscapeRoughness, Random.value, CashedGlobalMapChunks [1, 1].MatrixHeight [GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks [0, 0].MatrixHeight [GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks [1, 1].MatrixHeight [0, 0]);
		Generator.CreateHeightmap (CashedGlobalMapChunks [1, 0].MatrixForest, Generator.ForestRoughness, Random.value, CashedGlobalMapChunks [1, 1].MatrixForest [GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks [0, 0].MatrixForest [GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks [1, 1].MatrixForest [0, 0]);
		Generator.CreateRivers (CashedGlobalMapChunks [1, 0].MatrixHeight, CashedGlobalMapChunks [1, 0].MatrixRiver);

		CashedGlobalMapChunks [1, 2] = new Map (GlobalMapChunkSize);
		Generator.CreateHeightmap (CashedGlobalMapChunks [1, 2].MatrixHeight, Generator.LandscapeRoughness, CashedGlobalMapChunks [1, 1].MatrixHeight [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks [1, 1].MatrixHeight [0, GlobalMapChunkSize - 1], CashedGlobalMapChunks [0, 2].MatrixHeight [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
		Generator.CreateHeightmap (CashedGlobalMapChunks [1, 2].MatrixForest, Generator.ForestRoughness, CashedGlobalMapChunks [1, 1].MatrixForest [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks [1, 1].MatrixForest [0, GlobalMapChunkSize - 1], CashedGlobalMapChunks [0, 2].MatrixForest [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
		Generator.CreateRivers (CashedGlobalMapChunks [1, 2].MatrixHeight, CashedGlobalMapChunks [1, 2].MatrixRiver);

		CashedGlobalMapChunks [2, 0] = new Map (GlobalMapChunkSize);
		Generator.CreateHeightmap (CashedGlobalMapChunks [2, 0].MatrixHeight, Generator.LandscapeRoughness, Random.value, Random.value, CashedGlobalMapChunks [1, 0].MatrixHeight [GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks [1, 0].MatrixHeight [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
		Generator.CreateHeightmap (CashedGlobalMapChunks [2, 0].MatrixForest, Generator.ForestRoughness, Random.value, Random.value, CashedGlobalMapChunks [1, 0].MatrixForest [GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks [1, 0].MatrixForest [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
		Generator.CreateRivers (CashedGlobalMapChunks [2, 0].MatrixHeight, CashedGlobalMapChunks [2, 0].MatrixRiver);

		CashedGlobalMapChunks [2, 1] = new Map (GlobalMapChunkSize);
		Generator.CreateHeightmap (CashedGlobalMapChunks [2, 1].MatrixHeight, Generator.LandscapeRoughness, CashedGlobalMapChunks [2, 0].MatrixHeight [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks [1, 1].MatrixHeight [GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks [1, 1].MatrixHeight [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
		Generator.CreateHeightmap (CashedGlobalMapChunks [2, 1].MatrixForest, Generator.ForestRoughness, CashedGlobalMapChunks [2, 0].MatrixForest [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks [1, 1].MatrixForest [GlobalMapChunkSize - 1, 0], CashedGlobalMapChunks [1, 1].MatrixForest [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
		Generator.CreateRivers (CashedGlobalMapChunks [2, 1].MatrixHeight, CashedGlobalMapChunks [2, 1].MatrixRiver);

		CashedGlobalMapChunks [2, 2] = new Map (GlobalMapChunkSize);
		Generator.CreateHeightmap (CashedGlobalMapChunks [2, 2].MatrixHeight, Generator.LandscapeRoughness, CashedGlobalMapChunks [2, 1].MatrixHeight [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1], Random.value, CashedGlobalMapChunks [2, 1].MatrixHeight [0, GlobalMapChunkSize - 1], CashedGlobalMapChunks [1, 2].MatrixHeight [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1]);
		Generator.CreateHeightmap (CashedGlobalMapChunks [2, 2].MatrixForest, Generator.ForestRoughness, Random.value, Random.value, Random.value, Random.value);
		Generator.CreateRivers (CashedGlobalMapChunks [2, 2].MatrixHeight, CashedGlobalMapChunks [2, 2].MatrixRiver);

		LocalMaps = new Map[GlobalMapChunkSize, GlobalMapChunkSize];

		CurrentMap = CashedGlobalMapChunks [1, 1];

		Player = GameObject.FindWithTag ("Player");
		Visualiser.RenderVisibleHexes (Player.GetComponent<Player> ().MapCoords, Player.GetComponent<Player> ().ViewDistance, CashedGlobalMapChunks, ChunkY, ChunkX);
		Visualiser.HighlightHex(GetTopLeftMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetTopMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetTopRightMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetBottomRightMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetBottomMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetBottomLeftMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
	}

	public void OnGotoHex ()
	{
		if (CurrentMap == CashedGlobalMapChunks [1, 1]) 
		{
			float chunkX = Player.GetComponent<Player> ().MapCoords.x / GlobalMapChunkSize, chunkY = Player.GetComponent<Player> ().MapCoords.y / GlobalMapChunkSize;
			chunkX = Mathf.Floor (chunkX);
			chunkY = Mathf.Floor (chunkY);
			sbyte dx = (sbyte)(chunkX - ChunkX), dy = (sbyte)(chunkY - ChunkY);

			if (dx != 0) 
				for (sbyte i=-1; i<2; ++i) 
				{
					SaveChunk (ChunkY + i, ChunkX - dx, CashedGlobalMapChunks [1 + i, 1 - dx]);
					CashedGlobalMapChunks [1 + i, 1 - dx] = CashedGlobalMapChunks [1 + i, 1];
					CashedGlobalMapChunks [1 + i, 1] = CashedGlobalMapChunks [1 + i, 1 + dx];
					CashedGlobalMapChunks [1 + i, 1 + dx] = GetChunk (ChunkY + i, ChunkX + 2 * dx);
				}
			if (dy != 0) 
				for (sbyte i=-1; i<2; ++i) 
				{
					SaveChunk (ChunkY - dy, ChunkX + i, CashedGlobalMapChunks [1 - dy, 1 + i]);
					CashedGlobalMapChunks [1 - dy, 1 + i] = CashedGlobalMapChunks [1, 1 + i];
					CashedGlobalMapChunks [1, 1 + i] = CashedGlobalMapChunks [1 + dy, 1 + i];
					CashedGlobalMapChunks [1 + dy, 1 + i] = GetChunk (ChunkY + 2 * dy, ChunkX + i);
				}

			if (dx != 0 || dy != 0) 
				Debug.Log ("Done cashing chunks.");

			ChunkX += dx;
			ChunkY += dy;
			CurrentMap = CashedGlobalMapChunks [1, 1];

			Visualiser.RenderVisibleHexes (Player.GetComponent<Player> ().MapCoords, Player.GetComponent<Player> ().ViewDistance, CashedGlobalMapChunks, ChunkY, ChunkX);
		}
			Visualiser.DestroyAllObjects();//TODO Временно
			Visualiser.HighlightHex(GetTopLeftMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
			Visualiser.HighlightHex(GetTopMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
			Visualiser.HighlightHex(GetTopRightMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
			Visualiser.HighlightHex(GetBottomRightMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
			Visualiser.HighlightHex(GetBottomMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
			Visualiser.HighlightHex(GetBottomLeftMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
	}

	/// <summary>
	/// Переключение карт.
	/// </summary>
	public void SwitchMap ()
	{
		if (CurrentMap == CashedGlobalMapChunks [1, 1])
			GotoLocalMap ();
		else
			GotoGlobalMap ();
	}

	/// <summary>
	/// Переход на локальную карту.
	/// </summary>
	void GotoLocalMap ()
	{
		Vector2 mapCoords = Player.GetComponent<Player> ().MapCoords;
		if (LocalMaps [(int)mapCoords.y, (int)mapCoords.x] == null)
			CreateLocalMap (mapCoords, CashedGlobalMapChunks [1, 1].MatrixHeight [(int)mapCoords.y, (int)mapCoords.x], CashedGlobalMapChunks [1, 1].MatrixForest [(int)mapCoords.y, (int)mapCoords.x]);
		CurrentMap = LocalMaps [(int)mapCoords.y, (int)mapCoords.x];
		GlobalMapCoords = mapCoords;

		//TEMP
		Player.GetComponent<Player> ().MapCoords.x = CurrentMap.MatrixHeight.GetLength (0) / 2;
		Player.GetComponent<Player> ().MapCoords.y = CurrentMap.MatrixHeight.GetLength (0) / 2;
		//
		Visualiser.DestroyAllObjects();
		Visualiser.RenderWholeMap (CurrentMap);

		Visualiser.HighlightHex(GetTopLeftMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetTopMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetTopRightMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetBottomRightMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetBottomMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetBottomLeftMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
	}

	/// <summary>
	/// Переход на глобальную карту.
	/// </summary>
	void GotoGlobalMap ()
	{
		CurrentMap = CashedGlobalMapChunks [1, 1];
		Player.GetComponent<Player> ().MapCoords = GlobalMapCoords;
		Visualiser.RenderVisibleHexes (Player.GetComponent<Player> ().MapCoords, Player.GetComponent<Player> ().ViewDistance, CashedGlobalMapChunks, ChunkY, ChunkX);

		Visualiser.DestroyAllObjects();
		Visualiser.HighlightHex(GetTopLeftMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetTopMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetTopRightMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetBottomRightMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetBottomMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
		Visualiser.HighlightHex(GetBottomLeftMapCoords(Player.GetComponent<Player> ().MapCoords),Visualiser.BlueHexSprite);
	}

	/// <summary>
	/// Создаёт локальную карту.
	/// </summary>
	/// <param name="coords">Координаты новой карты на глобальной.</param>
	void CreateLocalMap (Vector2 mapCoords, float height, float forest)
	{
		Map map = new Map (LocalMapSize);

		Generator.CreateHeightmap (map.MatrixHeight, Generator.LandscapeRoughness, height, height, height, height);
		Generator.CreateHeightmap (map.MatrixForest, Generator.ForestRoughness, forest, forest, forest, forest);
		Generator.CreateRivers (map.MatrixHeight, map.MatrixRiver);

		LocalMaps [(int)mapCoords.y, (int)mapCoords.x] = map;
		//LocalMaps.Add (map);
		//return (uint)LocalMaps.Count - 1;
	}

	Map GetChunk (int chunkY, int chunkX)
	{
		Map chunk = new Map (GlobalMapChunkSize);
		if (TryGetChunk (chunkY, chunkX, chunk)) 
			return chunk;
		else
		{
			Vector2 topLeft, topRight, bottomLeft, bottomRight; // x - Height, y - Forest, River - ? TODO?
			Map neighbChunk = new Map (GlobalMapChunkSize);
			if (TryGetChunk (chunkY, chunkX - 1, neighbChunk))
			{
				topLeft.x = neighbChunk.MatrixHeight [0, GlobalMapChunkSize - 1];
				topLeft.y = neighbChunk.MatrixForest [0, GlobalMapChunkSize - 1];
				bottomLeft.x = neighbChunk.MatrixHeight [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1];
				bottomLeft.y = neighbChunk.MatrixForest [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1];
				if (TryGetChunk (chunkY, chunkX + 1, neighbChunk)) 
				{
					topRight.x = neighbChunk.MatrixHeight [0, 0];
					topRight.y = neighbChunk.MatrixForest [0, 0];
					bottomRight.x = neighbChunk.MatrixHeight [GlobalMapChunkSize - 1, 0];
					bottomRight.y = neighbChunk.MatrixForest [GlobalMapChunkSize - 1, 0];
					Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
					Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
					Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
					return chunk;
				}
				else
				{
					if (TryGetChunk (chunkY + 1, chunkX, neighbChunk)) 
					{
						topRight.x = neighbChunk.MatrixHeight [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1];
						topRight.y = neighbChunk.MatrixForest [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1];
						if (TryGetChunk (chunkY - 1, chunkX, neighbChunk)) 
						{
							bottomRight.x = neighbChunk.MatrixHeight [0, GlobalMapChunkSize - 1];
							bottomRight.y = neighbChunk.MatrixForest [0, GlobalMapChunkSize - 1];
							Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
							Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
							Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
							return chunk;
						}
						else
						{
							bottomRight.x = Random.value;
							bottomRight.y = Random.value;
							Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
							Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
							Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
							return chunk;
						}
					}
					else
					{
						topRight.x = Random.value;
						topRight.y = Random.value;
						if (TryGetChunk (chunkY - 1, chunkX, neighbChunk))
						{
							bottomRight.x = neighbChunk.MatrixHeight [0, GlobalMapChunkSize - 1];
							bottomRight.y = neighbChunk.MatrixForest [0, GlobalMapChunkSize - 1];
							Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
							Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
							Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
							return chunk;
						}
						else 
						{
							bottomRight.x = Random.value;
							bottomRight.y = Random.value;
							Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
							Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
							Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
							return chunk;
						}
					}
				}
			}
			else
			{
				if (TryGetChunk (chunkY, chunkX + 1, neighbChunk))
				{
					topRight.x = neighbChunk.MatrixHeight [0, 0];
					topRight.y = neighbChunk.MatrixForest [0, 0];
					bottomRight.x = neighbChunk.MatrixHeight [GlobalMapChunkSize - 1, 0];
					bottomRight.y = neighbChunk.MatrixForest [GlobalMapChunkSize - 1, 0];
					if (TryGetChunk (chunkY + 1, chunkX, neighbChunk))
					{
						topLeft.x = neighbChunk.MatrixHeight [GlobalMapChunkSize - 1, 0];
						topLeft.y = neighbChunk.MatrixForest [GlobalMapChunkSize - 1, 0];
						if (TryGetChunk (chunkY - 1, chunkX, neighbChunk))
						{
							bottomLeft.x = neighbChunk.MatrixHeight [0, 0];
							bottomLeft.y = neighbChunk.MatrixForest [0, 0];
							Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
							Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
							Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
							return chunk;
						}
						else 
						{
							bottomLeft.x = Random.value;
							bottomLeft.y = Random.value;
							Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
							Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
							Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
							return chunk;
						}
					}
					else 
					{
						topLeft.x = Random.value;
						topLeft.y = Random.value;
						if (TryGetChunk (chunkY - 1, chunkX, neighbChunk)) 
						{
							bottomLeft.x = neighbChunk.MatrixHeight [0, 0];
							bottomLeft.y = neighbChunk.MatrixForest [0, 0];
							Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
							Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
							Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
							return chunk;
						}
						else 
						{
							bottomLeft.x = Random.value;
							bottomLeft.y = Random.value;
							Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
							Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
							Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
							return chunk;
						}
					}
				}
				else
				{
					if (TryGetChunk (chunkY + 1, chunkX, neighbChunk))
					{
						topLeft.x = neighbChunk.MatrixHeight [GlobalMapChunkSize - 1, 0];
						topLeft.y = neighbChunk.MatrixForest [GlobalMapChunkSize - 1, 0];
						topRight.x = neighbChunk.MatrixHeight [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1];
						topRight.y = neighbChunk.MatrixForest [GlobalMapChunkSize - 1, GlobalMapChunkSize - 1];
						if (TryGetChunk (chunkY - 1, chunkX, neighbChunk)) 
						{
							bottomLeft.x = neighbChunk.MatrixHeight [0, 0];
							bottomLeft.y = neighbChunk.MatrixForest [0, 0];
							bottomRight.x = neighbChunk.MatrixHeight [0, GlobalMapChunkSize - 1];
							bottomRight.y = neighbChunk.MatrixForest [0, GlobalMapChunkSize - 1];
							Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
							Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
							Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
							return chunk;
						}
						else
						{
							bottomLeft.x = Random.value;
							bottomLeft.y = Random.value;
							bottomRight.x = Random.value;
							bottomRight.y = Random.value;
							Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
							Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
							Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
							return chunk;
						}
					}
					else 
					{
						topLeft.x = Random.value;
						topLeft.y = Random.value;
						topRight.x = Random.value;
						topRight.y = Random.value;
						if (TryGetChunk (chunkY - 1, chunkX, neighbChunk))
						{
							bottomLeft.x = neighbChunk.MatrixHeight [0, 0];
							bottomLeft.y = neighbChunk.MatrixForest [0, 0];
							bottomRight.x = neighbChunk.MatrixHeight [0, GlobalMapChunkSize - 1];
							bottomRight.y = neighbChunk.MatrixForest [0, GlobalMapChunkSize - 1];
							Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
							Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
							Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
							return chunk;
						} 
						else
						{
							bottomLeft.x = Random.value;
							bottomLeft.y = Random.value;
							bottomRight.x = Random.value;
							bottomRight.y = Random.value;
							Generator.CreateHeightmap (chunk.MatrixHeight, Generator.LandscapeRoughness, topLeft.x, topRight.x, bottomLeft.x, bottomRight.x);
							Generator.CreateHeightmap (chunk.MatrixForest, Generator.ForestRoughness, topLeft.y, topRight.y, bottomLeft.y, bottomRight.y);
							Generator.CreateRivers (chunk.MatrixHeight, chunk.MatrixRiver);
							return chunk;
						}
					}
				}
			}
		}
		
	}
	
	bool TryGetChunk (int chunkY, int chunkX, Map chunk)
	{
		if (Mathf.Abs (Mathf.Abs (chunkY) - Mathf.Abs (ChunkY)) <= 1 && Mathf.Abs (Mathf.Abs (chunkX) - Mathf.Abs (ChunkX)) <= 1)
		{
			for (sbyte y=-1; y<=1; ++y)
				for (sbyte x=-1; x<=1; ++x)
					if (chunkY == ChunkY + y && chunkX == ChunkX + x)
					{
						chunk = CashedGlobalMapChunks [y + 1, x + 1];
						return true;
					}
			return false; // Компиляция?
		}
		else
		{
			if (TryLoadFiledChunk (chunkY, chunkX, chunk))
				return true;
			else
				return false;
		}
	}

	bool TryLoadFiledChunk (int chunkY, int chunkX, Map chunk)
	{
		string filePath = Path.Combine (ChunksDirectoryPath, chunkY + "_" + chunkX);
		if (File.Exists (filePath)) 
		{
			using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open))) 
			{
				for (ushort y=0; y<GlobalMapChunkSize; ++y)
					for (ushort x=0; x<GlobalMapChunkSize; ++x)
						chunk.MatrixHeight [y, x] = reader.ReadSingle ();
			
				for (ushort y=0; y<GlobalMapChunkSize; ++y)
					for (ushort x=0; x<GlobalMapChunkSize; ++x)
						chunk.MatrixForest [y, x] = reader.ReadSingle ();
			
				for (ushort y=0; y<GlobalMapChunkSize; ++y)
					for (ushort x=0; x<GlobalMapChunkSize; ++x)
						chunk.MatrixRiver [y, x] = reader.ReadBoolean ();
			}
			return true;
		}
		return false;
	}
	
	void SaveChunk (int chunkY, int chunkX, Map chunk)
	{
		Directory.CreateDirectory (ChunksDirectoryPath);
		using (BinaryWriter writer = new BinaryWriter(File.Open(Path.Combine(ChunksDirectoryPath, chunkY+"_"+chunkX) , FileMode.Create)))
		{
			for (ushort y=0; y<GlobalMapChunkSize; ++y)
				for (ushort x=0; x<GlobalMapChunkSize; ++x)
					writer.Write (chunk.MatrixHeight [y, x]);

			for (ushort y=0; y<GlobalMapChunkSize; ++y)
				for (ushort x=0; x<GlobalMapChunkSize; ++x)
					writer.Write (chunk.MatrixForest [y, x]);

			for (ushort y=0; y<GlobalMapChunkSize; ++y)
				for (ushort x=0; x<GlobalMapChunkSize; ++x)
					writer.Write (chunk.MatrixRiver [y, x]);
		}
	}

	public Vector2 GetTopLeftMapCoords(Vector2 mapCoords)
	{
		return new Vector2(mapCoords.x-1,mapCoords.y-1+((mapCoords.x % 2) != 0 ? 1 : 0));
	}

	public Vector2 GetTopMapCoords(Vector2 mapCoords)
	{
		return new Vector2(mapCoords.x,mapCoords.y-1);
	}

	public Vector2 GetTopRightMapCoords(Vector2 mapCoords)
	{
		return new Vector2(mapCoords.x+1,mapCoords.y-1+((mapCoords.x % 2) != 0 ? 1 : 0));
	}

	public Vector2 GetBottomRightMapCoords(Vector2 mapCoords)
	{
		return new Vector2(mapCoords.x+1,mapCoords.y+((mapCoords.x % 2) != 0 ? 1 : 0));
	}

	public Vector2 GetBottomMapCoords(Vector2 mapCoords)
	{
		return new Vector2(mapCoords.x,mapCoords.y+1);
	}

	public Vector2 GetBottomLeftMapCoords(Vector2 mapCoords)
	{
		return new Vector2(mapCoords.x-1,mapCoords.y+((mapCoords.x % 2) != 0 ? 1 : 0));
	}

/// <summary>
/// Определяет, прилегают ли к друг другу данные координаты.
/// </summary>
/// <returns><c>true</c> если прилегают, иначе <c>false</c>.</returns>
/// <param name="mapCoords1">1 координаты.</param>
/// <param name="mapCoords2">2 координаты.</param>
	public bool IsMapCoordsAdjacent(Vector2 mapCoords1,Vector2 mapCoords2)
	{
		byte k=(byte)((mapCoords1.x % 2) != 0 ? 1 : 0);
		if(mapCoords1.x-1==mapCoords2.x||mapCoords1.x+1==mapCoords2.x)
		{
			if(mapCoords1.y-1+k==mapCoords2.y)
				return true;
			if(mapCoords1.y+k==mapCoords2.y)
					return true;
			return false;
		}
		if(mapCoords1.x==mapCoords2.x)
		{
			if(mapCoords1.y-1==mapCoords2.y)
				return true;
			if(mapCoords1.y+1==mapCoords2.y)
				return true;
			return false;
		}
		return false;
	}
}
