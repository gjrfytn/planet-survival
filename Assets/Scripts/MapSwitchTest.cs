using UnityEngine;
using System.Collections;

public class MapSwitchTest : MonoBehaviour
{
    private GameObject Player;
    private GameObject Camera;
    private GameObject World_;

    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        Camera = GameObject.FindWithTag("MainCamera");
        World_ = GameObject.FindWithTag("World");
    }

    public void OnClick()
    {
        World_.GetComponent<WorldVisualiser>().DestroyAllHexes(); //TODO Это лучше делать до генерации карты, чтобы не было видно подвисания (или нужно отображение загрузки).
        World_.GetComponent<World>().SwitchMap();
        Player.transform.position = new Vector3(Player.GetComponent<Player>().MapCoords.x * 0.48f, Player.GetComponent<Player>().MapCoords.y * 0.64f + ((Player.GetComponent<Player>().MapCoords.x % 2) != 0 ? 1 : 0) * 0.32f, -0.1f);
        Camera.transform.position = new Vector3(transform.position.x, transform.position.y + Camera.transform.position.z * (Mathf.Tan((360 - Camera.transform.rotation.eulerAngles.x) / 57.3f)), Camera.transform.position.z);
    }
}
