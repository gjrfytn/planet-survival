using UnityEngine;

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

	void CalculateMove(GlobalPos pos)
    {
        if (World.IsCurrentMapLocal())
        {
			if (Screen.width>>1 < WorldVisualiser.LocalHexSpriteSize.x * 100 * (Player.GetComponent<Player>().Pos.X) && Screen.width>>1 < WorldVisualiser.LocalHexSpriteSize.x * 100 * (World.LocalMapSize.X - Player.GetComponent<Player>().Pos.Y))
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
			if (Screen.height>> 1< WorldVisualiser.LocalHexSpriteSize.y * 75 * (Player.GetComponent<Player>().Pos.Y) && Screen.height>> 1 < WorldVisualiser.LocalHexSpriteSize.y * 75 * (World.LocalMapSize.Y - Player.GetComponent<Player>().Pos.Y))
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

	void FollowPlayer()//C#6.0 EBD
    {
        transform.position = new Vector3(MoveX ? Player.transform.position.x : transform.position.x, MoveY ? Player.transform.position.y : transform.position.y, transform.position.z);
    }
}
