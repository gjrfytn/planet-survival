using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Ingredient : MonoBehaviour {

    public Item Item;
    public Text RequiredAmountText;
    //Crafting Crafting;


	// Use this for initialization
	void Start () {

        //Crafting = GameObject.FindWithTag("InventoryManager").GetComponent<InventoryManager>().Crafting;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
