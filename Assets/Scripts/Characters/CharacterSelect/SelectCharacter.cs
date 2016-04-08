using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SelectCharacter : MonoBehaviour {
	

	public List<Transform> Marker;

	public Transform[] CharsPrefabs;

	public static Transform[] CharsPrefabsAux;

	private GameObject[] Chars;

	public Text CharacterNameText;

	public static int CurrentChar;
	
	void Start() 
	{
		CharsPrefabsAux = CharsPrefabs;

		Chars = new GameObject[CharsPrefabs.Length];

		int index = 0;
		foreach (Transform t in CharsPrefabs) 
		{
			Chars[index++] = GameObject.Instantiate(t.gameObject, Marker[4].position, Quaternion.identity) as GameObject;
		}

		Transform canvas = transform.parent;
		foreach(GameObject c in Chars)
		{
			c.transform.SetParent(canvas);
		}

	}
	void Update() 
	{
		CharacterNameText.text = Chars[CurrentChar].name;

		int current = CurrentChar = 0;	
		int backward = CurrentChar - 1;
		int forward = CurrentChar + 1;

		for (int i = 0; i < Chars.Length; i++) 
		{
			Transform transf = Chars[i].transform;

			if (i < backward) {
				transf.position = Vector3.Lerp(transf.position, Marker[0].position, 1);

			} else if (i > forward) {
				transf.position = Vector3.Lerp(transf.position, Marker[1].position, 1);

			} else if (i == backward) {
				transf.position = Vector3.Lerp(transf.position, Marker[2].position, 1);

			} else if (i == current) {
				transf.position = Vector3.Lerp(transf.position, Marker[3].position, 1);

			} else if (i == forward) {
				transf.position = Vector3.Lerp(transf.position, Marker[4].position, 1);
			}
		}
	}

	public void PreviousButton ()
	{
		CurrentChar--;
		
		if (CurrentChar < 0) 
		{
			CurrentChar = 0;
		}
	}
	public void NextButton ()
	{
		CurrentChar++;
		
		if (CurrentChar >= Chars.Length) 
		{
			CurrentChar = Chars.Length - 1;
		}
	}
	public void PickButton (string name)
	{
		StartCoroutine(LevelLoad("Game"));
	}
	
	
	IEnumerator LevelLoad(string name){
		yield return new WaitForSeconds(1f);
		Application.LoadLevel("LoadScreen");
	}
	

}
