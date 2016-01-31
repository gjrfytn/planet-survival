using UnityEngine;
using System.Collections;

public class InstantiateCharacter : MonoBehaviour {

	void Start () {
		// Клонируем персонажа которого выбрали в предидущей сцене
		GameObject.Instantiate(SelectCharacter.charsPrefabsAux[SelectCharacter.currentChar].gameObject, Vector2.zero, Quaternion.identity);
	}
	
}
