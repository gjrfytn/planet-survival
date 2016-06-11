using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SplitItem : MonoBehaviour {

    private Inventory Inventory;
    private int SplitSize;

    public InputField InputField;
    public Slider Slider;
	// Use this for initialization
	void Start () {

        Inventory = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>().Inventory;
        /*SplitSize = 1;
        Slider.minValue = 1;
        Slider.maxValue = ;//
        Slider.value = SplitSize;
        InputField.text = SplitSize.ToString();
        Slider.value = Slider.minValue;*/
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Accept()
    {

    }

    public void Cancel()
    {
        gameObject.SetActive(false);
    }
}
