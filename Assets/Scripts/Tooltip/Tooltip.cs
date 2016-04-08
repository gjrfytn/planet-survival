using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tooltip : MonoBehaviour {

  public GameObject TooltipPrefab;
  public float TooltipMainHeight;
  public float TooltipDelay;
  public Text TooltipText;
  public Text TooltipHeaderText;
  public Text TooltipDescriptionText;
  public Image TooltipHeader;
  public Color AttributeColor;

  [HideInInspector]
  public bool ShowTooltip;

	public IEnumerator Show_Tooltip(bool right, ItemClass item, SlotType type, int startNumber, RectTransform transform) 
	{
		
		ShowTooltip = true;
		
		if(right) 
		{
			TooltipPrefab.GetComponent<RectTransform>().pivot = new Vector2(1,1);
		}
		else 
		{
			TooltipPrefab.GetComponent<RectTransform>().pivot = new Vector2(0,1);
		}
		
		yield return new WaitForSeconds(TooltipDelay);
		
		if(ShowTooltip) 
		{
			foreach(Transform t in transform) 
			{
				t.gameObject.SetActive(true);
			}
			
			TooltipText.text = GenerateTooltip(item);
			
			TooltipPrefab.gameObject.SetActive(true);
			TooltipHeader.color = FindColor(item);
			TooltipHeaderText.text = item.TooltipHeader.ToUpper();
			TooltipDescriptionText.text = item.DescriptionText;

			
			RectTransform rect = TooltipPrefab.GetComponent<RectTransform>();
			
			TooltipPrefab.transform.position = transform.position;
			

			if(right) 
			{
				TooltipPrefab.GetComponent<RectTransform>().pivot = new Vector2(1,1);
				if(type == SlotType.Equipment) 
				{
					TooltipPrefab.transform.localPosition -= new Vector3(transform.sizeDelta.x * 0.5f, -transform.sizeDelta.y * 0.5f);
				}
			}
			else 
			{
				TooltipPrefab.GetComponent<RectTransform>().pivot = new Vector2(0,1);
				if(type == SlotType.Crafting) 
				{

				}
				else 
				{
					TooltipPrefab.transform.localPosition += new Vector3(transform.sizeDelta.x * item.Width,0);
					
				}
			}

			if(rect.localPosition.y < 0) 
			{
				if(Mathf.Abs(rect.localPosition.y) + rect.sizeDelta.y > transform.root.GetComponent<CanvasScaler>().referenceResolution.y * 0.5f) 
				{
					rect.localPosition -= new Vector3(0, transform.root.GetComponent<CanvasScaler>().referenceResolution.y * 0.5f - (Mathf.Abs(rect.localPosition.y) + rect.sizeDelta.y),0);
				}
			}
			else 
			{
				if(Mathf.Abs(rect.sizeDelta.y - Mathf.Abs(rect.localPosition.y)) > transform.root.GetComponent<CanvasScaler>().referenceResolution.y * 0.5f) 
				{
					rect.localPosition += new Vector3(0, Mathf.Abs(rect.sizeDelta.y - Mathf.Abs(rect.localPosition.y)) - transform.root.GetComponent<CanvasScaler>().referenceResolution.y * 0.5f,0);
				}
			}
		}
		else 
		{
			HideTooltip();
		}
	}
      
    public void HideTooltip()
    {
      ShowTooltip = false;
      foreach(Transform tr in transform)
        {
          tr.gameObject.SetActive(false);
        }
    }

    public string GenerateTooltip(ItemClass item)
    {
      string generatedTooltipText = "";
      Color color = FindColor(item);
      item.TooltipHeader = "<color=#" + ColorToHex(color) + ">" + item.ItemName + "</color>";

        return generatedTooltipText;
    }

  Color FindColor(ItemClass item)
  {
      Inventory inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
      if(item.ItemQuality == ItemQuality.Junk)
        {
          return inventory.JunkColor;
        }
      if(item.ItemQuality == ItemQuality.Normal)
        {
          return inventory.NormalColor;
        }
      if(item.ItemQuality == ItemQuality.Unusual)
        {
          return inventory.UnusualColor;
        }
      if(item.ItemQuality == ItemQuality.Rare)
        {
          return inventory.RareColor;
        }
      if(item.ItemQuality == ItemQuality.Legendary)
        {
          return inventory.LegendaryColor;
        }
        return Color.clear;
    }

    public static string ColorToHex(Color32 color)
    {
      string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
      return hex;
    }
}
