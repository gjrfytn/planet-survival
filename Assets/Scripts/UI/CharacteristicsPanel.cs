using UnityEngine;
using UnityEngine.UI;

public class CharacteristicsPanel : MonoBehaviour
{
    public Text Water;
    public Text Food;
    public Text Stamina;
    public Text Mental;

    void OnEnable()
    {
        Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
        //Пока всё без форматирования
        Water.text = player.Water.ToString() + "/" + player.MaxWater.ToString();
        Food.text = player.Food.ToString() + "/" + player.MaxFood.ToString();
        Stamina.text = player.Stamina.ToString() + "/" + player.MaxStamina.ToString();
        Mental.text = player.Mental.ToString() + "/" + player.MaxMental.ToString();
    }
}
