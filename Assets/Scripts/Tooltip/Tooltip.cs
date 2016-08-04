using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public Canvas Canvas;

    public GameObject TooltipPanel;

    [HideInInspector]
    public RectTransform CanvasRectTransform;
    [HideInInspector]
    public RectTransform RectTransform;

    public Image Image = null;

    public bool IsMultiline;

    public Text Text;


    void Awake()
    {
        CanvasRectTransform = Canvas.GetComponent<RectTransform>();
        RectTransform = TooltipPanel.GetComponent<RectTransform>();
    }
    // Use this for initialization
    public virtual void Start()
    {
        DeactivateTooltip();
    }


    public void ActivateTooltip()
    {
        TooltipPanel.SetActive(true);
    }

    public void DeactivateTooltip()
    {
        TooltipPanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData data)
    {

    }

    public void OnPointerExit(PointerEventData data)
    {

    }

}