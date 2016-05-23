using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
//using System.Linq;


public class World_Manager : EditorWindow
{
	[MenuItem("Game Modules/World Module/World Manager")]
	static void Init()
	{
		EditorWindow.GetWindow(typeof(World_Manager));

		
		
	}
	

	void Header()
	{
		GUILayout.BeginHorizontal();
		
		
		GUILayout.BeginVertical();
		GUILayout.Space(10);
		
		GUILayout.EndVertical();
		
		GUILayout.EndHorizontal();


		
	}

	#if UNITY_EDITOR
	[MenuItem("Game Modules/World Module/Create/World")]   
	public static void CreateWorld()  
	{
		GameObject World = GameObject.FindGameObjectWithTag("World");
		if (World == null)
		{
			World = (GameObject)Instantiate(Resources.Load("Prefabs/MapGenerator/World") as GameObject);         
		}
		else
			Debug.Log("Мир уже создан");
	}
	#endif
	
}