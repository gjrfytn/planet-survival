using UnityEngine;
using System.Collections;

public class MainCamera : MonoBehaviour 
{
	GameObject Player;
	World World;

	bool MoveX=true,MoveY=true;

	void OnEnable()
	{
		EventManager.PlayerMoved+=CalculateMove;
		EventManager.PlayerObjectMoved+=FollowPlayer;
	}
	
	void OnDisable()
	{
		EventManager.PlayerMoved-=CalculateMove;
		EventManager.PlayerObjectMoved-=FollowPlayer;
	}

	void Start () 
	{
		Player=GameObject.FindWithTag("Player");
		World=GameObject.FindWithTag("World").GetComponent<World>();
	}

	void CalculateMove(Vector2 playerMCoords)
	{
		if(World.IsCurrentMapLocal())
		{
			if(Screen.width/2<WorldVisualiser.HexSpriteSize.x*75*(Player.GetComponent<Player>().MapCoords.x-2)&&Screen.width/2<WorldVisualiser.HexSpriteSize.x*75*(World.LocalMapSize.x-Player.GetComponent<Player>().MapCoords.x-3))
				MoveX=true;
			else
				MoveX=false;
			if(Screen.height/2<WorldVisualiser.HexSpriteSize.y*100*(Player.GetComponent<Player>().MapCoords.y-1)&&Screen.height/2<WorldVisualiser.HexSpriteSize.y*100*(World.LocalMapSize.y-Player.GetComponent<Player>().MapCoords.y-1))
				MoveY=true;
			else
				MoveY=false;
		}
		else
		{
			MoveX=true;
			MoveY=true;
		}
	}

	void FollowPlayer()
	{
		transform.position=new Vector3(MoveX?Player.transform.position.x:transform.position.x,MoveY?Player.transform.position.y:transform.position.y,transform.position.z);
	}
}
