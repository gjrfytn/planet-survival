using UnityEngine;
using System.Collections;

public class EnemyInteraction : MonoBehaviour
{
    GameObject Player;
    GameObject Camera;
    World World_;
    GameObject GameEventManager;
    EventManager EventManager;

    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        Camera = GameObject.FindWithTag("MainCamera");
        World_ = GameObject.FindWithTag("World").GetComponent<World>();
        GameEventManager = GameObject.FindWithTag("GameEventManager");
        EventManager = GameObject.Find("EventManager").GetComponent<EventManager>();
    }


    public void OnMouseUpAsButton()
    {
        /*if(World.IsMapCoordsAdjacent(Player.GetComponent<Player>().MapCoords,GetComponent<HexData>().MapCoords))
        {
            Player.GetComponent<Player>().MoveToMapCoords(GetComponent<HexData>().MapCoords);
            EventManager.OnTurn();
            Camera.transform.position=new Vector3(transform.position.x,transform.position.y+Camera.transform.position.z*(Mathf.Tan((360-Camera.transform.rotation.eulerAngles.x)/57.3f)),Camera.transform.position.z);
            World_.OnGotoHex();
            if(!World_.IsCurrentMapLocal())
                GameEventManager.GetComponent<GameEventManager>().MakeActionEvent();
        }*/
    }

    public void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * 1.2f;
    }

    public void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * 0.833f;
    }
}
