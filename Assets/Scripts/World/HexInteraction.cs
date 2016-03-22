using UnityEngine;
using System.Collections;

public class HexInteraction : MonoBehaviour
{
    public void OnMouseUpAsButton()
    {
        // Раскомментировать if, если нужна проверка, соседний ли это гекс.
        //if (World.IsMapCoordsAdjacent(Player.GetComponent<Player>().MapCoords, GetComponent<HexData>().MapCoords))
        // {
        GameObject.FindWithTag("Player").GetComponent<Player>().MoveToMapCoords(GetComponent<HexData>().MapCoords);
        if (!GameObject.FindWithTag("World").GetComponent<World>().IsCurrentMapLocal())
            GameObject.FindWithTag("GameEventManager").GetComponent<GameEventManager>().MakeActionEvent(GameObject.FindWithTag("World").GetComponent<World>().GetHexTerrain(GetComponent<HexData>().MapCoords));
        // }
    }

    public void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * 1.5f;
    }

    public void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * 0.666f;
    }
}
