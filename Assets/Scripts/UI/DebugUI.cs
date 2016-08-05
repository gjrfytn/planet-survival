using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    [SerializeField]
    Slider Health;
    [SerializeField]
    Slider Stamina;
    [SerializeField]
    Slider Water;
    [SerializeField]
    Slider Food;

    Player Player;

    void Start()
    {
        Player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }

    void Update() //TODO По событиям?
    {
        Health.value = (float)Player.Health / Player.MaxHealth;
        Stamina.value = Player.Stamina / Player.MaxStamina;
        Water.value = Player.Water / Player.MaxWater;
        Food.value = Player.Food / Player.MaxFood;
    }
}
