using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ItemClassController : MonoBehaviour {

	public ItemClass Item;
	public LayerMask Mask;
	public LookAt LookAt;
	public Inventory Inventory;

	void Awake() {

		gameObject.AddComponent<Rigidbody2D>();
		MeshCollider col = gameObject.AddComponent<MeshCollider>();
		col.convex = true;
	}

	// Use this for initialization
	void Start () {
	
		LookAt = GetComponentInChildren<LookAt>();
		Inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
		OnMouseExit();

	}

	void OnMouseOver()
	{

		if(Input.GetMouseButtonDown(1))
		{
			if(Item.Stackable)
			{
				Inventory.AddStackableItem(Item);
			}
			else
			{
				Inventory.AddItem(Item);
			}
			DestroyObject(gameObject);
		}

	}

	void OnMouseEnter()
	{
		LookAt.Show = true;
		foreach(Transform tr in transform)
		{
			tr.gameObject.SetActive(true);
		}
		Text text = GetComponentInChildren<Text>();
		text.text = Item.ItemName;
		text.color = Inventory.FindColor(Item);

		Image image = GetComponentInChildren<Image>();
		image.rectTransform.sizeDelta = new Vector2(text.preferredWidth + 12, text.preferredHeight + 8);
	}

	void OnMouseExit()
	{

		LookAt.Show = false;
		foreach(Transform tr in transform)
		{
			tr.gameObject.SetActive(false);
		}

	}
}
