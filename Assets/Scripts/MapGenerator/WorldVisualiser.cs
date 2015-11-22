using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldVisualiser : MonoBehaviour
{
	public GameObject Hex;
	public Sprite Grass;
	public Sprite Sand;
	public Sprite River;
	public Sprite Mud;
	public Sprite Mountain;
	public GameObject Tree;

	//private List<GameObject> Hexes=new List<GameObject>(16384);//TODO Оценить, сколько нужно выделять, чтобы повысить эффективность.
	//private List<GameObject> Trees=new List<GameObject>(5000);  //То же самое.

	//private World World_;

	//private List<GameObject> RenderedHexes = new List<GameObject> ();
	//private List<bool> Mask;
	private class ListType
	{
		public GameObject Hex;
		public bool InSign;
	}

	private List<ListType> RenderedHexes = new List<ListType> ();
	private Map Map;

	private class QueueType
	{
		public Vector2 Position;
		public byte Distance;
	}

	private Queue<QueueType> SignQueue = new Queue<QueueType> ();
//	void Start()
//	{
//		World_=GetComponent<World>();
//	}
	
	public void RenderVisibleHexes (Vector2 position, byte distance, Map currentMap)
	{
		Map = currentMap;

		RenderedHexes.ForEach(hex=>hex.InSign=false);

		SignQueue.Enqueue (new QueueType{Position=position, Distance=distance});
		while (SignQueue.Count!=0)
		{
			QueueType buf = SignQueue.Dequeue ();
			SpreadRender (buf.Position, buf.Distance);
		}

		for (ushort i=0; i<RenderedHexes.Count; ++i)
			if (!RenderedHexes [i].InSign) 
			{
				Destroy (RenderedHexes [i].Hex);
				RenderedHexes.RemoveAt (i);
				i--;
			}
	}

	void SpreadRender (Vector2 position, byte distance)
	{
		if (position.y < 0 || position.x < 0 || position.y >= Map.MatrixHeight.GetLength (0) || position.x >= Map.MatrixHeight.GetLength (0))
			return;

		short index = (short)RenderedHexes.FindIndex (x => x.Hex.GetComponent<HexData> ().MapCoords == position);
		if (index == -1)
		{
			Quaternion rot = new Quaternion ();
			ListType hex = new ListType{Hex= Instantiate (Hex, new Vector2 (position.x * 0.96f, position.y * 0.64f + ((position.x % 2) != 0 ? 1 : 0) * 0.32f), rot) as GameObject,InSign= true};
			hex.Hex.GetComponent<HexData> ().MapCoords = position;
			ChooseSprite (hex.Hex, position);
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
			byte k = (byte)((position.x % 2) != 0 ? 1 : 0); // Учитываем чётность/нечётность ряда хексов

			SignQueue.Enqueue (new QueueType{Position=new Vector2 (position.x - 1, position.y - 1 + k),Distance=(byte)(distance - 1)});

			SignQueue.Enqueue (new QueueType{Position=new Vector2 (position.x - 1, position.y + k),Distance= (byte)(distance - 1)});

			SignQueue.Enqueue (new QueueType{Position=new Vector2 (position.x, position.y - 1), Distance=(byte)(distance - 1)});

			SignQueue.Enqueue (new QueueType{Position=new Vector2 (position.x, position.y + 1), Distance=(byte)(distance - 1)});

			SignQueue.Enqueue (new QueueType{Position=new Vector2 (position.x + 1, position.y - 1 + k),Distance=(byte) (distance - 1)});

			SignQueue.Enqueue (new QueueType{Position=new Vector2 (position.x + 1, position.y + k), Distance=(byte)(distance - 1)});
		}
	}

	void ChooseSprite (GameObject hex, Vector2 mapCoords)
	{
		if (Map.MatrixRiver [(int)mapCoords.y, (int)mapCoords.x])
			hex.GetComponent<SpriteRenderer> ().sprite = River;
		else if (Map.MatrixHeight [(int)mapCoords.y, (int)mapCoords.x] > 0.6f && Map.MatrixHeight [(int)mapCoords.y, (int)mapCoords.x] < 0.9f)
			hex.GetComponent<SpriteRenderer> ().sprite = Sand;
		else if (Map.MatrixHeight [(int)mapCoords.y, (int)mapCoords.x] < 0.1f)
			hex.GetComponent<SpriteRenderer> ().sprite = Mud;
		else if (Map.MatrixHeight [(int)mapCoords.y, (int)mapCoords.x] > 0.9f)
			hex.GetComponent<SpriteRenderer> ().sprite = Mountain;
		else
			hex.GetComponent<SpriteRenderer> ().sprite = Grass;
	}

	/*
	/// <summary>
	/// Выводит хексы карты на сцену.
	/// </summary>
	/// <param name="map">Карта.</param>
	public void RenderNewMap(Map map)
	{
		ushort size=(ushort)map.MatrixHeight.GetLength(0);

		if(size!=GetComponent<WorldGenerator>().GlobalMapSize)
		{
			for(ushort i=4097;i<12315;++i)
				Destroy(Hexes[i]);
			Hexes.RemoveRange(4097,12315); //TODO Временно
		}

		Vector2 pos = new Vector2 (0, 0);
		Quaternion rot = new Quaternion ();
		ushort halfwidth = (ushort)(size / 2);

		ushort hexIndex=0;
		for (int x=-halfwidth; x<halfwidth; ++x) {
			pos.x = x * 0.96f;
			//pos.x = x;
			for (ushort y=4; y<size+4; ++y) {
				//pos.y = y + ((x % 2)!=0?1:0) * 0.5f;
				pos.y = y * 0.64f + ((x % 2) != 0 ? 1 : 0) * 0.32f; //Смещаем каждый нечётный столбец хексов

				GameObject newHex;
				if(Hexes.Count==0)
					newHex = Instantiate (Hex, pos, rot) as GameObject;
					else
				{
					newHex=Hexes[hexIndex];
					hexIndex++;
				}
				if (map.MatrixRiver [y - 4, x + halfwidth]) 
					newHex.GetComponent<SpriteRenderer> ().sprite = River;
				else if (map.MatrixHeight [y - 4, x + halfwidth] > 0.6f && map.MatrixHeight [y - 4, x + halfwidth] < 0.9f)
					newHex.GetComponent<SpriteRenderer> ().sprite = Sand;
				else if (map.MatrixHeight [y - 4, x + halfwidth] < 0.1f)
					newHex.GetComponent<SpriteRenderer> ().sprite = Mud;
				else if (map.MatrixHeight [y - 4, x + halfwidth] > 0.9f)
					newHex.GetComponent<SpriteRenderer> ().sprite = Mountain;
				else 
					newHex.GetComponent<SpriteRenderer> ().sprite = Grass;
				//				switch (Mathf.RoundToInt (Matrix [x + 32, y - 4])) 
				//				{
				//				case 0:
				//					Instantiate (hexGrass, pos, rot);
				//					break;
				//				case 1:
				//					Instantiate (hexSand, pos, rot);
				//					break;
				//				}
			}
		}

		foreach (GameObject tree in Trees)
			Destroy(tree);
		Trees.Clear();


		for (int x=-halfwidth; x<halfwidth; ++x) 
		{
			pos.x = x * 0.96f;
			//pos.x = x;
			for (ushort y=4; y<size+4; ++y) {
				//pos.y = y + ((x % 2)!=0?1:0) * 0.5f;
				pos.y = y * 0.64f + ((x % 2) != 0 ? 1 : 0) * 0.32f; //Смещаем каждый нечётный столбец хексов
				
				for(byte i=0;i<map.MatrixForest[y-4,x+halfwidth]*40;++i)
				{
					Vector2 v=Random.insideUnitCircle;
					v.x*=0.6f;
					v.y*=0.3f;
					Vector3 vec=new Vector3(v.x+pos.x,v.y+pos.y,-0.1f);
					Instantiate(Tree,vec,rot);
				}
			}
		}
	}
	 */

}
