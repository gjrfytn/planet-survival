using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class LocalMapEditor : MonoBehaviour {

	public ushort MinMapSize;
	public ushort MaxMapSize;

	private ushort X;
	private ushort Y;

	public InputField InputFieldX;
	public InputField InputFieldY;

	[Range(0f, 0.6f)]
	public float TerrainHeight;


	// Use this for initialization
	void Start () {

		InputFieldX.text = X.ToString();
		InputFieldY.text = Y.ToString();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		
	public void DestroyMap()
	{
		GameObject.FindWithTag("WorldVisualizer").GetComponent<WorldVisualizer>().DestroyAllHexes();
	}

	public void CreateLocalMap(GameObject panel)
	{
		InputMapSize();

		if(X >= MinMapSize && Y >= MinMapSize && X <= MaxMapSize && Y <= MaxMapSize)
		{
			//Очищаем карту перед созданием новой
			DestroyMap();

			LocalMap map = new LocalMap(X, Y);

			for(ushort i = 0; i < map.Width; ++i)
			{
				for(ushort j = 0; j < map.Height; ++j)
				{
					//map.HeightMatrix[j, i] = TerrainHeight;
				}
			}
			GameObject.FindWithTag("WorldVisualizer").GetComponent<WorldVisualizer>().RenderWholeMap(map);

			panel.gameObject.SetActive(false);
		}

		if(X < MinMapSize || Y < MinMapSize)
		{
			Debug.Log("Меньше " + MinMapSize);
		}

		if(X > MaxMapSize || Y > MaxMapSize)
		{
			Debug.Log("Больше " + MaxMapSize);
		}

	}

	public void InputMapSize()
	{
			X = ushort.Parse(InputFieldX.text);
			Y = ushort.Parse(InputFieldY.text);
	}

}
