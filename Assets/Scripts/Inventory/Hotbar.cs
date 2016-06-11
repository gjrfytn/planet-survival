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
            Slots.Add(SlotContainer.GetChild(i).gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
