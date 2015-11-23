using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotCover : MonoBehaviour
{

    Inventory inv;
    RectTransform rT;

    void Start()
    {
        inv = transform.parent.parent.parent.parent.GetComponent<Inventory>();
        rT = GetComponent<RectTransform>();

    }

    void Update()
    {
        rT.sizeDelta = new Vector3(inv.slotSize, inv.slotSize, 0);
    }
}
