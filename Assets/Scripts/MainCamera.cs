using UnityEngine;
using System.Collections;

public class MainCamera : MonoBehaviour
{
    GameObject Player;
    World World;

    bool MoveX = true, MoveY = true;

    void OnEnable()
    {
        EventManager.PlayerMoved += CalculateMove;
        EventManager.PlayerObjectMoved += FollowPlayer;
    }

    void OnDisable()
    {
        EventManager.PlayerMoved -= CalculateMove;
        EventManager.PlayerObjectMoved -= FollowPlayer;
    }

    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        World = GameObject.FindWithTag("World").GetComponent<World>();
    }

    void CalculateMove(Vector2 playerMCoords)
    {
        if (World.IsCurrentMapLocal())
        {
            if (Screen.width / 2 < WorldVisualiser.LocalHexSpriteSize.x * 100 * (Player.GetComponent<Player>().MapCoords.x - 1) && Screen.width / 2 < WorldVisualiser.LocalHexSpriteSize.x * 100 * (World.LocalMapSize.x - Player.GetComponent<Player>().MapCoords.x - 1))
                MoveX = true;
            else
                MoveX = false;
            if (Screen.height / 2 < WorldVisualiser.LocalHexSpriteSize.y * 75 * (Player.GetComponent<Player>().MapCoords.y - 2) && Screen.height / 2 < WorldVisualiser.LocalHexSpriteSize.y * 75 * (World.LocalMapSize.y - Player.GetComponent<Player>().MapCoords.y - 3))
                MoveY = true;
            else
                MoveY = false;
        }
        else
        {
            MoveX = true;
            MoveY = true;
        }
    }

    void FollowPlayer()
    {
        transform.position = new Vector3(MoveX ? Player.transform.position.x : transform.position.x, MoveY ? Player.transform.position.y : transform.position.y, transform.position.z);
    }
}
