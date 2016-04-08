using UnityEngine;
using System.Collections;

public class HexInteraction : MonoBehaviour
{
    World World;

    void Start()
    {
        World = GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World;
    }

    public void OnMouseUpAsButton()
    {
        // Раскомментировать if, если нужна проверка, соседний ли это гекс.
        //if (World.IsMapCoordsAdjacent(Player.GetComponent<Player>().MapCoords, GetComponent<HexData>().MapCoords))
        // {
        if (!World.IsCurrentMapLocal())
        {
            //TerrainType terrType = (World.CurrentMap as GlobalMap).GetTerrainType( GetComponent<HexData>().MapCoords);
            EventManager.OnPlayerMove(GetComponent<HexData>().MapCoords); //Временно см. Player 114
            TerrainType terrType = World.GetHexTerrain(GetComponent<HexData>().MapCoords);
            ushort time = 0;
            foreach (Terrains.TerrainProperties p in GameObject.FindWithTag("World").GetComponent<Terrains>().TerrainsArray)
                if ((terrType & p.Terrain) != 0)
                    time += p.TravelTime;
            GameObject.FindWithTag("Player").GetComponent<Player>().MoveToMapCoords(GetComponent<HexData>().MapCoords, time * GameTime.GameMinToRealSec);
            EventManager.OnActionStart(time);
        }
        else
            GameObject.FindWithTag("Player").GetComponent<Player>().MoveToMapCoords(GetComponent<HexData>().MapCoords);
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
