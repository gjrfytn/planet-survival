using UnityEngine;
using System.Collections.Generic;

public class ActionGroups : MonoBehaviour
{
    public enum ActionGroup { ENEMY, CONTAINER }

    [System.Serializable]
    public class ActionGroupProperties
    {
        public ActionGroup ActionGroup;
        public Action[] Actions;
    }

    public ActionGroupProperties[] ActionGroupsArray;

    void Awake()
    {
        for (byte i = 0; i < ActionGroupsArray.Length; ++i)
            for (byte j = 0; j < ActionGroupsArray.Length; ++j)
                if (ActionGroupsArray[i].ActionGroup == ActionGroupsArray[j].ActionGroup && i != j)
                    throw new System.Exception("Duplicatated action groups in ActionGroupsArray.");
    }

    public Action[] GetActions(ActionGroup ag)
    {
        foreach (ActionGroupProperties p in ActionGroupsArray)
            if (p.ActionGroup == ag)
                return p.Actions;
        throw new System.ArgumentException("Action group not found.", "ag");
    }

    //На будущее
    /*public Dictionary<ActionGroup,Action[]> Groups;

    void Start()
    {
        Player player = UnityEngine.GameObject.FindWithTag("Player").GetComponent<Player>();
        World world = UnityEngine.GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World;

        ActionGroups = new Dictionary<ActionGroup, Action[]>
        {
            {ActionGroup.ENEMY,new Action[]
                {
                    new Action(0,0,0,0,new List<EffectApplier>()
                        {
                            new EffectApplier(
                        },new List<EffectApplier>()
                        {

                        }),
                    new Action()
                }},
            {ActionGroup.CONTAINER,new Action[]
                {
                    new Action(),
                    new Action()
                }}
        };
    }*/
}
