using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hotbar : MonoBehaviour {

    public Transform SlotContainer;

    public List<GameObject> Slots;

	// Use this for initialization
	void Start () {
	
        for(int i = 0; i < SlotContainer.childCount; i++)
        {
            if (SlotContainer.GetChild(i).GetComponent<Slot>().SlotType == SlotType.Hotbar)
            {
                Slots.Add(SlotContainer.GetChild(i).gameObject);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
