using UnityEngine;
using System.Collections;

public class MainCamera : MonoBehaviour
{
    GameObject Player;
    World World;

    bool MoveX = true, MoveY = true;
	bool CapturedX=false,CapturedY=false;

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
        World = GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World;
    }

    void CalculateMove(Vector2 playerMCoords)
    {
        if (World.IsCurrentMapLocal())
        {
            if (Screen.width / 2 < WorldVisualiser.LocalHexSpriteSize.x * 100 * (Player.GetComponent<Player>().MapCoords.x) && Screen.width / 2 < WorldVisualiser.LocalHexSpriteSize.x * 100 * (World.LocalMapSize.x - Player.GetComponent<Player>().MapCoords.x))
			{
				if(CapturedX)
					CapturedX=false;
				else
					MoveX = true;
			}
            else
			{
                MoveX = false;
				CapturedX=true;
			}
            if (Screen.height / 2 < WorldVisualiser.LocalHexSpriteSize.y * 75 * (Player.GetComponent<Player>().MapCoords.y) && Screen.height / 2 < WorldVisualiser.LocalHexSpriteSize.y * 75 * (World.LocalMapSize.y - Player.GetComponent<Player>().MapCoords.y))
			{
				if(CapturedY)
					CapturedY=false;
				else
					MoveY = true;
			}
            else
			{
                MoveY = false;
				CapturedY=true;
			}
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
