using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class Tooltip : MonoBehaviour
{

    public GameObject TooltipPanel;

    public RectTransform CanvasRectTransform;
    public RectTransform RectTransform;

    //public int TooltipHeight;
    //public int TooltipWidth;

    public Text HeaderText;
    public Text DescriptionText;
    public Text AdditionalText;

    public Image Image = null;

    public GameObject CustomBody = null;

    public GameObject ScrollContent = null;

    // Use this for initialization
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {


    }


    public void ActivateTooltip()
    {
        TooltipPanel.SetActive(true);
    }

    public void DeactivateTooltip()
    {
        TooltipPanel.SetActive(false);
    }


}