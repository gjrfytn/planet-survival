using UnityEngine;
using System.Collections;

public class MapSwitchButton : MonoBehaviour
{
    GameObject Player;
    GameObject World;

    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        World = GameObject.FindWithTag("World");
    }

    public void Click()
    {
        World.GetComponent<WorldVisualiser>().DestroyAllHexes(); //TODO Это лучше делать до генерации карты, чтобы не было видно подвисания (или нужно отображение загрузки).
        World.GetComponent<World>().SwitchMap();
        Player.transform.position = new Vector3(WorldVisualiser.GetTransformPosFromMapCoords(Player.GetComponent<Player>().MapCoords).x, WorldVisualiser.GetTransformPosFromMapCoords(Player.GetComponent<Player>().MapCoords).y, 0f); //TODO
        Camera.main.transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y + Camera.main.transform.position.z * (Mathf.Tan((360 - Camera.main.transform.rotation.eulerAngles.x) / 57.3f)), Camera.main.transform.position.z);
    }
}
