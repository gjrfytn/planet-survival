using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemOnObject : MonoBehaviour       
{
    public Item item;           
    private Text text;                       
    private Image image;

    void Update()
    {
        text.text = "" + item.itemValue;                    
        image.sprite = item.itemIcon;
        GetComponent<ConsumeItem>().item = item;
    }

    void Start()
    {
        image = transform.GetChild(0).GetComponent<Image>();
        transform.GetChild(0).GetComponent<Image>().sprite = item.itemIcon;  
        text = transform.GetChild(1).GetComponent<Text>();   
    }
}
