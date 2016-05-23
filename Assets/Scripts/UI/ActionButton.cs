using UnityEngine;

public class ActionButton : MonoBehaviour
{
    public Entity Entity;

    public void Click()
    {
        Entity.GetComponent<PopupButtons>().ButtonClick(gameObject);
        GameObject.FindWithTag("Player").GetComponent<Player>().Attack(Entity as Creature);
    }
}
