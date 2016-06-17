using UnityEngine;
using System.Collections.Generic;

public class EntityAction : MonoBehaviour
{
    public Action[] Actions;

    Action[] AllActions;

    bool Clicked;

    Player Player;

    void Start()
    {
        Player = GameObject.FindWithTag("Player").GetComponent<Player>();

        AllActions = new Action[0];//Временно
        /*Action[] enemybuf=new Action[0];
        Action[] containerbuf=new Action[0];
        if(GetComponent<LivingBeing>()!=null)
            enemybuf=GameObject.FindWithTag("SharedActions").GetComponent<ActionGroups>().GetActions(ActionGroups.ActionGroup.ENEMY);
        if(GetComponent<Container>()!=null)
            containerbuf=GameObject.FindWithTag("SharedActions").GetComponent<ActionGroups>().GetActions(ActionGroups.ActionGroup.CONTAINER);
        AllActions=new Action[enemybuf.Length+containerbuf.Length+Actions.Length];
        for(byte i=0;i<enemybuf.Length;++i)
            AllActions[i]=enemybuf[i];
        for(byte i=0;i<containerbuf.Length;++i)
            AllActions[enemybuf.Length+i]=containerbuf[i];
        for(byte i=0;i<Actions.Length;++i)
            AllActions[enemybuf.Length+containerbuf.Length+i]=Actions[i];*/
    }

    void OnMouseUpAsButton()
    {
        if (Clicked)
        {
            EventManager.PopupButtonClicked -= ButtonClick;
            EventManager.OnPopupButtonExpel();
        }
        else
        {
            EventManager.PopupButtonClicked += ButtonClick;

            Action[] filteredActions;
            if ((GetComponent<Creature>().AggressiveTo & Creature.AggrTarget.PLAYER) != Creature.AggrTarget.NONE)
            {
                TempWeapon weapon = Player.GetWeapon();
                filteredActions = new Action[]{
					new BattleAction(){Sprite=weapon.NormalHitSprite,Damage=weapon.NormalHitDamage,StaminaCost=weapon.NormalHitStaminaCost,Accuracy=0.75f},
					new BattleAction(){Sprite=weapon.PowerHitSprite,Damage=weapon.PowerHitDamage,StaminaCost=weapon.PowerHitStaminaCost,Accuracy=0.5f},
					new BattleAction(){Sprite=weapon.RareHitSprite,Damage=weapon.RareHitDamage,StaminaCost=weapon.RareHitStaminaCost,Accuracy=0.25f}
				};
            }
            else
                filteredActions = AllActions;
            EventManager.OnPopupButtonsCall(transform.position, filteredActions);
        }
        Clicked = !Clicked;
    }

    void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * 1.2f;
    }

    void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * 0.833f;
    }

    void ButtonClick(PopupButton btn)
    {
        if (Clicked)
        {
            BattleAction ba = btn.Action as BattleAction;
            GameObject.FindWithTag("Player").GetComponent<Player>().Attack(GetComponent<LivingBeing>(), ba.Damage, ba.Accuracy);
            EventManager.PopupButtonClicked -= ButtonClick;
            EventManager.OnPlayerTurn();
            Clicked = false;
        }
    }
}
