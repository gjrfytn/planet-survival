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
	public Vector2 HexSpriteSize;
	public GameObject Tree;
	public byte ForestDensity;

	public float FadeInSpeed;
	public float FadeSpeed;
	//private World World_;
	
	private class ListType
	{
		public GameObject Hex;
		public bool InSign;
		public List<GameObject> Trees=new List<GameObject>();
	}

	private List<ListType> RenderedHexes = new List<ListType> ();
	private Map Map;

	private class QueueType
	{
		public Vector2 Position;
		public byte Distance;
	}

	private Queue<QueueType> SignQueue = new Queue<QueueType> ();
	void Awake()
	{
		//TODO 100 - число пикселей на единицу (свойство спрайта), возможно стоит это число брать из самих спрайтов
		HexSpriteSize.x/=100;
		HexSpriteSize.y/=100;
	}
	
	public void RenderVisibleHexes (Vector2 mapPosition, byte distance, Map currentMap)
	{
		Map = currentMap;

		RenderedHexes.ForEach(hex=>hex.InSign=false);

		SignQueue.Enqueue (new QueueType{Position=mapPosition, Distance=distance});
		while (SignQueue.Count!=0)
		{
			QueueType buf = SignQueue.Dequeue ();
			SpreadRender (buf.Position, buf.Distance);
		}

		for (ushort i=0; i<RenderedHexes.Count; ++i)
			if (!RenderedHexes [i].InSign) 
			{
			RenderedHexes[i].Trees.ForEach(tree=>StartCoroutine(FadeAndDestroy(tree)));
			StartCoroutine(FadeAndDestroy(RenderedHexes [i].Hex));
				RenderedHexes.RemoveAt (i);
				i--;
			}
	}

	//TODO Корутины загружают процессор?

	IEnumerator FadeIn(GameObject obj) 
	{
		SpriteRenderer renderer=obj.GetComponent<SpriteRenderer>();
		Color cbuf=renderer.material.color;
		cbuf.a=0;
		renderer.material.color=cbuf;

		while(renderer.material.color.a<1)
		{
			Color buf=renderer.material.color;
			buf.a+=FadeInSpeed;
			renderer.material.color=buf;
			yield return null;
		}
	}

	IEnumerator FadeAndDestroy(GameObject obj) 
	{
		SpriteRenderer renderer=obj.GetComponent<SpriteRenderer>();

		while(renderer.material.color.a>0)
		{
			Color buf=renderer.material.color;
			buf.a-=FadeSpeed;
			renderer.material.color=buf;
			yield return null;
		}
		Destroy(obj);
	}

	void SpreadRender (Vector2 mapPosition, byte distance)
	{
		if (mapPosition.y < 0 || mapPosition.x < 0 || mapPosition.y >= Map.MatrixHeight.GetLength (0) || mapPosition.x >= Map.MatrixHeight.GetLength (0))
			return;

		short index = (short)RenderedHexes.FindIndex (x => x.Hex.GetComponent<HexData> ().MapCoords == mapPosition);
		if (index == -1)
		{
			Quaternion rot = new Quaternion ();
			ListType hex = new ListType{Hex= Instantiate (Hex, new Vector2 (mapPosition.x * HexSpriteSize.x*0.75f, mapPosition.y * HexSpriteSize.y + ((mapPosition.x % 2) != 0 ? 1 : 0) * HexSpriteSize.y*0.5f), rot) as GameObject,InSign= true};
			hex.Hex.GetComponent<HexData>().MapCoords = mapPosition;
			MakeHexGraphics (hex, mapPosition);
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

	void MakeHexGraphics(ListType hex, Vector2 mapCoords)
	{
		ChooseHexSprite(hex.Hex,mapCoords);
		StartCoroutine( FadeIn(hex.Hex));
		MakeHexForest(hex,mapCoords);
	}

	void ChooseHexSprite (GameObject hex, Vector2 mapCoords)
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

	void MakeHexForest (ListType hex, Vector2 mapCoords)
	{
		Quaternion rot=new Quaternion();
		for(byte i=0;i<Map.MatrixForest[(int)mapCoords.y,(int)mapCoords.x]*ForestDensity;++i)
	{
		Vector2 v=Random.insideUnitCircle;
			v.x*=HexSpriteSize.x*0.5f;
			v.y*=HexSpriteSize.y*0.5f;
			Vector3 vec=new Vector3(v.x+hex.Hex.transform.position.x,v.y+hex.Hex.transform.position.y,-0.1f);
			hex.Trees.Add(Instantiate(Tree,vec,rot) as GameObject);
			StartCoroutine( FadeIn(hex.Trees[hex.Trees.Count-1]));
	}
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
