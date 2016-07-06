using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EntityActions : MonoBehaviour
{
    public Action[] Actions;

    Action[] AllActions;

    Player Player;

    void Start()
    {
        Player = GameObject.FindWithTag("Player").GetComponent<Player>();

        Action[] enemybuf = new Action[0];
        Action[] containerbuf = new Action[0];
        if (GetComponent<LivingBeing>() != null)
            enemybuf = GameObject.FindWithTag("SharedActions").GetComponent<ActionGroups>().GetActions(ActionGroups.ActionGroup.ENEMY);
        if (GetComponent<Container>() != null)
            containerbuf = GameObject.FindWithTag("SharedActions").GetComponent<ActionGroups>().GetActions(ActionGroups.ActionGroup.CONTAINER);
        AllActions = new Action[enemybuf.Length + containerbuf.Length + Actions.Length];
        for (byte i = 0; i < enemybuf.Length; ++i)
            AllActions[i] = enemybuf[i];
        for (byte i = 0; i < containerbuf.Length; ++i)
            AllActions[enemybuf.Length + i] = containerbuf[i];
        for (byte i = 0; i < Actions.Length; ++i)
            AllActions[enemybuf.Length + containerbuf.Length + i] = Actions[i];
    }

    void OnMouseUpAsButton()
    {
        EventManager.OnPopupButtonsCall(transform.position, FilterActions(Player.Weapon), false);
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(0) && AttachedItem.DraggingItem != null)
            EventManager.OnPopupButtonsCall(transform.position, FilterActions(AttachedItem.DraggingItem.GetComponent<AttachedItem>().Item), false);
    }

    Action[] FilterActions(Item item)
    {
        foreach (Action a in AllActions)
            if (a is AttackAction)
            {
                a.AdditionalSprite = item.Icon;
                (a as AttackAction).Target = GetComponent<LivingBeing>();
            }
        Action[] filteredActions;
        if (GetComponent<Creature>() != null && (GetComponent<Creature>().AggressiveTo & Creature.AggrTarget.PLAYER) != Creature.AggrTarget.NONE)
            filteredActions = AllActions.Where(a => a is AttackAction).ToArray();
        else
            filteredActions = AllActions;

        return filteredActions.Where(a => a.RequiredItemActionTypes.Length == 0 || a.RequiredItemActionTypes.Contains(item.ItemActionType)).ToArray();
    }

    void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * 1.2f;
    }

    void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * 0.833f;
    }
}
