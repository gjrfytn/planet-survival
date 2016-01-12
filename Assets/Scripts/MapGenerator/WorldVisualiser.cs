﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldVisualiser : MonoBehaviour
{
	[System.Serializable]
	public class Terrain
	{
		public float StartingHeight;
		public Sprite Sprite_;
	}

	public GameObject Hex;
	public Sprite BottommostTerrainSprite;
	public Terrain[] Terrains;
	public Sprite River;
	public Vector2 HexSpriteSize;
	public GameObject Tree;
	public byte ForestDensity;
	public byte ForestGenGridSize;
	public float FadeInSpeed;
	public float FadeSpeed;
	
	class ListType
	{
		public GameObject Hex;
		public bool InSign;
		public List<GameObject> Trees = new List<GameObject> ();
	}

	List<ListType> RenderedHexes = new List<ListType> ();
	//Map Map;
	Map[,] CashedChunks = new Map[3, 3];
	int ChunkX, ChunkY;


	class QueueType
	{
		public Vector2 Position;
		public byte Distance;
	}

	Queue<QueueType> SignQueue = new Queue<QueueType> ();

	void Awake ()
	{
		for (byte i=1; i<Terrains.Length; i++)
			Debug.Assert (Terrains [i - 1].StartingHeight < Terrains [i].StartingHeight);

		//TODO 100 - число пикселей на единицу (свойство спрайта), возможно стоит это число брать из самих спрайтов
		HexSpriteSize.x /= 100;
		HexSpriteSize.y /= 100;
	}

	/// <summary>
	/// Уничтожает все хексы.
	/// </summary>
	public void DestroyAllHexes ()
	{
		RenderedHexes.ForEach (hex => {
			hex.Trees.ForEach (tree => Destroy (tree));
			Destroy (hex.Hex);});
		RenderedHexes.Clear ();
	}

	/// <summary>
	/// Отображает только хексы в поле зрения игрока.
	/// </summary>
	/// <param name="mapPosition">Координаты в матрице.</param>
	/// <param name="distance">Дальность обзора.</param>
	/// <param name="currentMap">Активная карта.</param>
	public void RenderVisibleHexes (Vector2 mapPosition, byte distance, Map[,] cashedChunks, int chunkY, int chunkX)
	{
		CashedChunks = cashedChunks;
		ChunkX = chunkX;
		ChunkY = chunkY;

		RenderedHexes.ForEach (hex => hex.InSign = false);

		SignQueue.Enqueue (new QueueType{Position=mapPosition, Distance=distance});
		while (SignQueue.Count!=0)
		{
			QueueType buf = SignQueue.Dequeue ();
			SpreadRender (buf.Position, buf.Distance);
		}

		for (ushort i=0; i<RenderedHexes.Count; ++i)
			if (!RenderedHexes [i].InSign)
			{
				RenderedHexes [i].Trees.ForEach (tree => StartCoroutine (FadeAndDestroy (tree)));
				StartCoroutine (FadeAndDestroy (RenderedHexes [i].Hex));
				RenderedHexes.RemoveAt (i);
				i--;
			}
	}

	/// <summary>
	/// Постепенно отображает объект (корутина).
	/// </summary>
	/// <returns>(Корутина).</returns>
	/// <param name="obj">Объект для отображения.</param>
	IEnumerator FadeIn (GameObject obj)
	{
		SpriteRenderer renderer = obj.GetComponent<SpriteRenderer> ();
		Color cbuf = renderer.material.color;
		cbuf.a = 0;
		renderer.material.color = cbuf;

		while (renderer!=null&&renderer.material.color.a<1) 
		{
			Color buf = renderer.material.color;
			buf.a += FadeInSpeed;
			renderer.material.color = buf;
			yield return null;
		}
	}

	/// <summary>
	/// Постепенно скрывает, затем уничтожает объект (корутина).
	/// </summary>
	/// <returns>(Корутина).</returns>
	/// <param name="obj">Объект к уничтожению.</param>
	IEnumerator FadeAndDestroy (GameObject obj)
	{
		SpriteRenderer renderer = obj.GetComponent<SpriteRenderer> ();

		while (renderer.material.color.a>0)
		{
			Color buf = renderer.material.color;
			buf.a -= FadeSpeed;
			renderer.material.color = buf;
			yield return null;
		}
		Destroy (obj);
	}

	/// <summary>
	/// Рекурсивно заносит в очередь на отображение хексы.
	/// </summary>
	/// <param name="mapPosition">Координаты в матрице.</param>
	/// <param name="distance">Оставшееся расстояние для распространения.</param>
	void SpreadRender (Vector2 mapPosition, byte distance)
	{
		Map map;
		Vector2 chunkPos;
		ushort chunkSize = (ushort)CashedChunks [1, 1].MatrixHeight.GetLength (0);

		float chunkX = mapPosition.x / chunkSize, chunkY = mapPosition.y / chunkSize;

		chunkX = Mathf.Floor (chunkX);
		chunkY = Mathf.Floor (chunkY);

		map = CashedChunks [(int)(chunkY - ChunkY + 1), (int)(chunkX - ChunkX + 1)];

		chunkPos.x = mapPosition.x - chunkSize * chunkX;
		chunkPos.y = mapPosition.y - chunkSize * chunkY;

		short index = (short)RenderedHexes.FindIndex (x => x.Hex.GetComponent<HexData> ().MapCoords == mapPosition);
		if (index == -1) 
		{
			Quaternion rot = new Quaternion ();
			ListType hex = new ListType{Hex= Instantiate (Hex, new Vector2 (mapPosition.x * HexSpriteSize.x*0.75f, mapPosition.y * HexSpriteSize.y + ((mapPosition.x % 2) != 0 ? 1 : 0) * HexSpriteSize.y*0.5f), rot) as GameObject,InSign= true};
			hex.Hex.GetComponent<HexData> ().MapCoords = mapPosition;
			MakeHexGraphics (hex, chunkPos, map);
			RenderedHexes.Add (hex);
		}
		else
		{
			if (RenderedHexes [index].InSign)
				return;
			else
				RenderedHexes [index].InSign = true;
		}

		if (distance != 0) 
		{
			byte k = (byte)((mapPosition.x % 2) != 0 ? 1 : 0); // Учитываем чётность/нечётность ряда хексов

			SignQueue.Enqueue (new QueueType{Position=new Vector2 (mapPosition.x - 1, mapPosition.y - 1 + k),Distance=(byte)(distance - 1)});

			SignQueue.Enqueue (new QueueType{Position=new Vector2 (mapPosition.x - 1, mapPosition.y + k),Distance= (byte)(distance - 1)});

			SignQueue.Enqueue (new QueueType{Position=new Vector2 (mapPosition.x, mapPosition.y - 1), Distance=(byte)(distance - 1)});

			SignQueue.Enqueue (new QueueType{Position=new Vector2 (mapPosition.x, mapPosition.y + 1), Distance=(byte)(distance - 1)});

			SignQueue.Enqueue (new QueueType{Position=new Vector2 (mapPosition.x + 1, mapPosition.y - 1 + k),Distance=(byte) (distance - 1)});

			SignQueue.Enqueue (new QueueType{Position=new Vector2 (mapPosition.x + 1, mapPosition.y + k), Distance=(byte)(distance - 1)});
		}
	}

	/// <summary>
	/// Создаёт спрайты, необходимые для отображения хекса.
	/// </summary>
	/// <param name="hex">Хекс.</param>
	/// <param name="mapCoords">Координаты в матрице.</param>
	void MakeHexGraphics (ListType hex, Vector2 mapCoords, Map map)
	{
		ChooseHexSprite (hex.Hex, mapCoords, map);
		StartCoroutine (FadeIn (hex.Hex));
		MakeHexForest (hex, mapCoords, map);
	}
	
	/// <summary>
	/// Выбирает спрайт хекса.
	/// </summary>
	/// <param name="hex">Хекс.</param>
	/// <param name="mapCoords">Координаты в матрице.</param>
	void ChooseHexSprite (GameObject hex, Vector2 mapCoords, Map map) //UNDONE
	{
		if (map.MatrixRiver [(int)mapCoords.y, (int)mapCoords.x])
			hex.GetComponent<SpriteRenderer> ().sprite = River;
		else 
		{
			if (map.MatrixHeight [(int)mapCoords.y, (int)mapCoords.x] < Terrains [0].StartingHeight)
			{
				hex.GetComponent<SpriteRenderer> ().sprite = BottommostTerrainSprite;
				return;
			}
			for (byte i=1; i<Terrains.Length; i++)
				if (map.MatrixHeight [(int)mapCoords.y, (int)mapCoords.x] >= Terrains [i - 1].StartingHeight && map.MatrixHeight [(int)mapCoords.y, (int)mapCoords.x] < Terrains [i].StartingHeight) 
				{
					hex.GetComponent<SpriteRenderer> ().sprite = Terrains [i - 1].Sprite_;
					return;
				}
			if (map.MatrixHeight [(int)mapCoords.y, (int)mapCoords.x] >= Terrains [Terrains.Length - 1].StartingHeight)
				hex.GetComponent<SpriteRenderer> ().sprite = Terrains [Terrains.Length - 1].Sprite_;
		}
	}

	/// <summary>
	/// Создаёт лес на хексе.
	/// </summary>
	/// <param name="hex">Хекс.</param>
	/// <param name="mapCoords">Координаты в матрице.</param>
	void MakeHexForest (ListType hex, Vector2 mapCoords, Map map) //TODO (WIP)
	{
		if (map.MatrixForest [(int)mapCoords.y, (int)mapCoords.x] * ForestDensity >= 1) 
		{
			Quaternion rot = new Quaternion ();
			float gridStepX = HexSpriteSize.x / ForestGenGridSize;
			float gridStepY = HexSpriteSize.y / ForestGenGridSize;
			Vector2 gridOrigin = new Vector2 (hex.Hex.transform.position.x - HexSpriteSize.x * 0.375f, hex.Hex.transform.position.y - HexSpriteSize.y * 0.5f);
			byte treesCount = (byte)(map.MatrixForest [(int)mapCoords.y, (int)mapCoords.x] * ForestDensity);

			while (true) 
			{
				if (treesCount > ForestGenGridSize * ForestGenGridSize) 
				{
					for (float y=0; y<HexSpriteSize.y; y+=gridStepY)
						for (float x=0; x<HexSpriteSize.x; x+=gridStepX) 
						{
							Vector2 v = new Vector2 (Random.value * gridStepX, Random.value * gridStepY);
							hex.Trees.Add (Instantiate (Tree, new Vector3 (gridOrigin.x + x + v.x, gridOrigin.y + y + v.y, -0.1f), rot) as GameObject);
							StartCoroutine (FadeIn (hex.Trees [hex.Trees.Count - 1]));
							treesCount--;
						}
				}
				else
				{
					Vector2 v = Random.insideUnitCircle;
					v.x *= HexSpriteSize.x * 0.5f;
					v.y *= HexSpriteSize.y * 0.5f;
					hex.Trees.Add (Instantiate (Tree, new Vector3 (hex.Hex.transform.position.x + v.x, hex.Hex.transform.position.y + v.y, -0.1f), rot) as GameObject);
					StartCoroutine (FadeIn (hex.Trees [hex.Trees.Count - 1]));
					treesCount--;
					if (treesCount == 0)
						return;
				}
			}
		}
	}
	
	/// <summary>
	/// Выводит хексы карты на сцену.
	/// </summary>
	/// <param name="map">Карта.</param>
	public void RenderWholeMap (Map map)
	{
		ushort size = (ushort)map.MatrixHeight.GetLength (0);
		RenderedHexes.Capacity = size * size;
		Quaternion rot = new Quaternion ();

		for (ushort y=0; y<size; ++y)
			for (ushort x=0; x<size; ++x) 
			{
				// TODO Возможно стоит заменить ListType на Hex?
				ListType hex = new ListType{Hex= Instantiate (Hex, new Vector2 (x * HexSpriteSize.x*0.75f, y * HexSpriteSize.y + ((x % 2) != 0 ? 1 : 0) * HexSpriteSize.y*0.5f), rot) as GameObject,InSign= true};
				hex.Hex.GetComponent<HexData> ().MapCoords = new Vector2 (y, x);
				MakeHexGraphics (hex, new Vector2 (y, x), map);
				RenderedHexes.Add (hex);
			}
	}
}