using UnityEngine;

public class HexInteraction : MonoBehaviour
{
    World World;

    void Start()
    {
        World = GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World;
    }

    public void OnMouseUpAsButton()
    {
        if (!World.IsCurrentMapLocal())
        {
            //TerrainType terrType = (World.CurrentMap as Chunk).GetTerrainType( GetComponent<HexData>().MapCoords);
            EventManager.OnPlayerMove(GetComponent<HexData>().Pos); //Временно см. Player 114
            TerrainType terrType = World.GetHexTerrain(GetComponent<HexData>().Pos);
            TimedAction travel = new TimedAction();
            foreach (Terrains.TerrainProperties p in GameObject.FindWithTag("World").GetComponent<Terrains>().TerrainsArray)
                if ((terrType & p.Terrain) != 0)
                {
                    travel.Duration += p.Travel.Duration;
                    travel.WaterConsumption += p.Travel.WaterConsumption;
                    travel.FoodConsumption += p.Travel.FoodConsumption;
                    travel.EnergyConsumption += p.Travel.EnergyConsumption;
                }
            GameObject.FindWithTag("Player").GetComponent<Player>().MoveTo(GetComponent<HexData>().Pos, travel.Duration * GameTime.GameMinToRealSec);
            EventManager.OnActionStart(travel);
        }
        else
            GameObject.FindWithTag("Player").GetComponent<Player>().MoveTo((LocalPos)GetComponent<HexData>().Pos, true);
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
