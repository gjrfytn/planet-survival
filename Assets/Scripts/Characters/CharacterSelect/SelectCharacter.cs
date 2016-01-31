using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SelectCharacter : MonoBehaviour {
	

	public Transform[] marker;

	public Transform[] charsPrefabs;

	public static Transform[] charsPrefabsAux;

	private GameObject[] chars;

	public Text CharacterNameText;

	public static int currentChar = 1;
	
	void Start() 
	{
		charsPrefabsAux = charsPrefabs;

		chars = new GameObject[charsPrefabs.Length];

		int index = 0;
		foreach (Transform t in charsPrefabs) 
		{
			chars[index++] = GameObject.Instantiate(t.gameObject, marker[4].position, Quaternion.identity) as GameObject;
		}

		Transform canvas = transform.parent;
		foreach(GameObject c in chars)
			c.transform.SetParent(canvas);


	}
	void Update() {
		CharacterNameText.text = chars[currentChar].name;

		int current = currentChar;	
		int backward = currentChar - 1;
		int forward = currentChar + 1;

		for (int index = 0; index < chars.Length; index++) 
		{
			Transform transf = chars[index].transform;

			if (index < backward) {
				transf.position = Vector3.Lerp(transf.position, marker[0].position, 1);

			} else if (index > forward) {
				transf.position = Vector3.Lerp(transf.position, marker[1].position, 1);

			} else if (index == backward) {
				transf.position = Vector3.Lerp(transf.position, marker[2].position, 1);

			} else if (index == current) {
				transf.position = Vector3.Lerp(transf.position, marker[3].position, 1);

			} else if (index == forward) {
				transf.position = Vector3.Lerp(transf.position, marker[4].position, 1);
			}
		}
	}

	public void PreviousButton ()
	{
		currentChar--;
		
		if (currentChar < 0) 
		{
			currentChar = 0;
		}
	}
	public void NextButton ()
	{
		currentChar++;
		
		if (currentChar >= chars.Length) 
		{
			currentChar = chars.Length - 1;
		}
	}
	public void PickButton (string name)
	{
		StartCoroutine(LevelLoad("Game"));
	}
	
	
	IEnumerator LevelLoad(string name){
		yield return new WaitForSeconds(1f);
		Application.LoadLevel("Game");
	}
	

}
