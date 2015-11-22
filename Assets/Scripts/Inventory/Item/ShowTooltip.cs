using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShowTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{  

    public Tooltip tooltip;                  
    public GameObject tooltipGameObject;                       
    public RectTransform canvasRectTransform;         
    public RectTransform tooltipRectTransform;      
    private Item item;


    void Start()
    {
        if (GameObject.FindGameObjectWithTag("Tooltip") != null)
        {
            tooltip = GameObject.FindGameObjectWithTag("Tooltip").GetComponent<Tooltip>();
            tooltipGameObject = GameObject.FindGameObjectWithTag("Tooltip");
            tooltipRectTransform = tooltipGameObject.GetComponent<RectTransform>() as RectTransform;
        }
        canvasRectTransform = GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>() as RectTransform;
    }




    public void OnPointerEnter(PointerEventData data)     
    {
        if (tooltip != null)
        {
            item = GetComponent<ItemOnObject>().item;    
            tooltip.item = item;       
            tooltip.activateTooltip();         
            if (canvasRectTransform == null)
                return;


            Vector3[] slotCorners = new Vector3[4];   
            GetComponent<RectTransform>().GetWorldCorners(slotCorners);             

            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, slotCorners[3], data.pressEventCamera, out localPointerPosition)) 
            {
                if (transform.parent.parent.parent.GetComponent<Hotbar>() == null)
                    tooltipRectTransform.localPosition = localPointerPosition; 
                else
                    tooltipRectTransform.localPosition = new Vector3(localPointerPosition.x, localPointerPosition.y + tooltip.tooltipHeight);
            }

        }

    }

    public void OnPointerExit(PointerEventData data)     
    {
        if (tooltip != null)
            tooltip.deactivateTooltip();    
    }

}
