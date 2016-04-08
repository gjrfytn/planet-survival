using UnityEngine;
using System.Collections;

public class UseEffect : MonoBehaviour {

	public ItemClass Item;
	public Player Player;
	public Inventory Inventory;

	// Use this for initialization
	void Start () {
	
		Player = GameObject.FindGameObjectWithTag ("Player").GetComponent<Player>();
		Inventory = GameObject.FindGameObjectWithTag ("Inventory").GetComponent<Inventory>();
	}

	public virtual string Use()
	{
		return "";
	}
}
