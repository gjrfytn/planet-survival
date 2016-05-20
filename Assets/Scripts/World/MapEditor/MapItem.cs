using UnityEngine;
using System.Collections;

[System.Serializable]
public class MapItem {

	public string Name;
	public int ID;
	public GameObject ItemPrefab;
	public int Width;
	public int Height;
	public bool IsInteractable;
	public bool Tooltip;


}
