using UnityEngine;
using System.Collections;

public class HexInteraction : MonoBehaviour
{
    GameObject Player;
    World World_;
    GameObject GameEventManager;

    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        World_ = GameObject.FindWithTag("World").GetComponent<World>();
        GameEventManager = GameObject.FindWithTag("GameEventManager");
    }

    public void OnMouseUpAsButton()
    {
        if (World.IsMapCoordsAdjacent(Player.GetComponent<Player>().MapCoords, GetComponent<HexData>().MapCoords))
        {
            Player.GetComponent<Player>().MoveToMapCoords(GetComponent<HexData>().MapCoords);
            EventManager.OnTurn();
            World_.OnGotoHex();
            //if (!World_.IsCurrentMapLocal())
            //    GameEventManager.GetComponent<GameEventManager>().MakeActionEvent();
        }
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
