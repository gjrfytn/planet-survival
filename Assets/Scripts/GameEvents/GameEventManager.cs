using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class GameEventManager : MonoBehaviour
{
    public GameObject GameEventTimer;
    public GameObject EventPanel;
    public GameObject ReactionButtonPanel;
    public GameObject Btn;
	const string EventsFilename="events.edb";
    List<GameEvent> Events = new List<GameEvent>();
    List<GameObject> ReactionButtons = new List<GameObject>();
    GameEvent CurrentEvent;

    void Start()
    {
		LoadEventsFromFile(Path.Combine(Application.streamingAssetsPath,EventsFilename)); //UNDONE Не будет работать на Android?
    }

    public void MakeActionEvent()
    {
        //Событие при переходе игрока на тайл.
        List<GameEvent> possibleEvents = Events.Where(evnt => evnt.ByAction).ToList();// UNDONE Не учитываются коэффициенты.

        List<GameEvent> eventLst = possibleEvents.GetRange(0, possibleEvents.Count);
        while (eventLst.Count != 0)
        {
            GameEvent evnt = eventLst[Random.Range(0, eventLst.Count)];
            if (Random.value < evnt.Probability) // UNDONE Не учитываются коэффициенты.
            {
                CurrentEvent = evnt;
                EventPanel.SetActive(true);
				EventPanel.transform.GetChild(0).GetComponent<Text>().text = evnt.Description;//ParseEventDescription(evnt.Description); TODO
                sbyte d = (sbyte)(evnt.Reactions.Count - ReactionButtons.Count - 1);
                if (d > 0)
                    for (byte i = 0; i < d; ++i)
                    {
                        GameObject newBtn = Instantiate(Btn);
                        newBtn.GetComponent<EventButtonTest>().Index = (byte)(ReactionButtons.Count != 0 ? ReactionButtons[ReactionButtons.Count - 1].GetComponent<EventButtonTest>().Index + 1 : 1);
                        newBtn.transform.SetParent(ReactionButtonPanel.transform);
                        ReactionButtons.Add(newBtn);
                    }
                else if (d < 0)
                    for (sbyte i = -1; i >= d; --i)
                    {
                        Destroy(ReactionButtons[ReactionButtons.Count + i]);
                        ReactionButtons.RemoveAt(ReactionButtons.Count + i);
                    }
                //ReactionButtons.RemoveRange(ReactionButtons.Count-Mathf.Abs(d),Mathf.Abs(d));

                Btn.transform.GetChild(0).GetComponent<Text>().text = evnt.Reactions[0].Description;
                for (byte i = 0; i < ReactionButtons.Count; ++i)
                    ReactionButtons[i].transform.GetChild(0).GetComponent<Text>().text = evnt.Reactions[i + 1].Description;
                return;
            }
            else
                eventLst.Remove(evnt);
        }
    }

    public void MakeTimeEvent()
    {
        //TODO Событие от таймера.
    }

    public void EventPanelButtonPress(byte index)
    {
        GameEvent.Reaction.Result result = null;
        bool resultFound = false;
        while (!resultFound)
        {
            List<GameEvent.Reaction.Result> resultLst = CurrentEvent.Reactions[index].Results.GetRange(0, CurrentEvent.Reactions[index].Results.Count);
            while (resultLst.Count != 0)
            {
                /*GameEvent.Reaction.Result*/
                result = resultLst[Random.Range(0, resultLst.Count)];
                if (Random.value < result.Probability)
                {
                    resultFound = true;
                    break;
                }
                else
                    resultLst.Remove(result);
            }
        }
		Dictionary<string,float?> effects;
		Debug.Log(ParseResultDescription(result.Description,out effects));
		foreach(KeyValuePair<string,float?> effect in effects)
		{
			if(effect.Value.HasValue)
				EventEffects.ApplyEffect(effect.Key,effect.Value.Value);
			else
				EventEffects.ApplyEffect(effect.Key);
		}
        EventPanel.SetActive(false);
        CurrentEvent = null;
    }

    void LoadEventsFromFile(string filename)
    {
		if (File.Exists(filename))
        {
			using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                while (reader.PeekChar() != -1)
                {
                    string name = reader.ReadString();
                    bool good = reader.ReadBoolean();
                    bool byAction = reader.ReadBoolean();
                    bool byTime = reader.ReadBoolean();
                    string description = reader.ReadString();
                    float probability = reader.ReadSingle();

                    GameEvent.TerrainCoefficients terrCoef = new GameEvent.TerrainCoefficients()
                    {
                        ForestСoef = reader.ReadSingle(),
                        RiverСoef = reader.ReadSingle()
                    };
                    GameEvent.TimeCoefficients timeCoef = new GameEvent.TimeCoefficients()
                    {
                        DayСoef = reader.ReadSingle(),
                        NightСoef = reader.ReadSingle()
                    };
                    GameEvent.StateCoefficients stateCoef = new GameEvent.StateCoefficients();
                    //

                    byte reactionCount = reader.ReadByte();
                    List<GameEvent.Reaction> reactions = new List<GameEvent.Reaction>(reactionCount);
                    for (byte i = 0; i < reactionCount; ++i)
                    {
                        string reacDesc = reader.ReadString();

                        byte resultCount = reader.ReadByte();
                        List<GameEvent.Reaction.Result> results = new List<GameEvent.Reaction.Result>(resultCount);
                        for (byte j = 0; j < resultCount; ++j)
                        {
                            results.Add(new GameEvent.Reaction.Result()
                                        {
                                            Description = reader.ReadString(),
                                            Probability = reader.ReadSingle()
                                        });
                        }

                        reactions.Add(new GameEvent.Reaction()
                                      {
                                          Description = reacDesc,
                                          Results = results
                                      });
                    }
                    Events.Add(new GameEvent(name, good, byAction, byTime, description, probability, terrCoef, timeCoef, stateCoef, reactions));
                }
            }
        }
        else
            throw new IOException("File with events do not exist.");
    }

    string ParseEventDescription(string description)
    {
        string parsedDesc = description;
        for (ushort i = 0; i < parsedDesc.Length; ++i)
            if (parsedDesc[i] == '<')
            {
                string tag = string.Empty;
                byte j;
                for (j = 1; parsedDesc[i + j] != '>'; ++j)
                    tag += parsedDesc[i + j];
                parsedDesc = parsedDesc.Remove(i, j + 1);
                if (tag.EndsWith("!"))
                {
                    tag = tag.Remove(tag.Length - 1);
				string replacement= EventDictionary.TagDictionary[tag]+' ';
				parsedDesc = parsedDesc.Insert(i, replacement);
				i += (ushort)replacement.Length;
                }
                else
                {
                   
                }
                //TODO Применение эффекта.
            }
        return parsedDesc;
    }

    string ParseResultDescription(string description, out Dictionary<string,float?> effects)
    {
        string parsedDesc = description;
		effects=new Dictionary<string, float?>();
        for (ushort i = 0; i < parsedDesc.Length; ++i)
            if (parsedDesc[i] == '<')
            {
                string tag = string.Empty;
                float? value=null;
                byte j;
                for (j = 1; parsedDesc[i + j] != '>'; ++j)
                {
                    if(parsedDesc[i+j]=='=')
                    {
                        string strValue=string.Empty;
                        for(++j;parsedDesc[i+j]!='>';++j)
                        {
                            if(parsedDesc[i+j]=='!')
                            {
                                tag+=parsedDesc[i+j];
                                ++j;
                                break;
                            }
                            strValue+=parsedDesc[i+j];
                        }
                        value=float.Parse(strValue);
                        break;
                    }
                    tag += parsedDesc[i + j];
                }
                parsedDesc = parsedDesc.Remove(i, j + 1);
                if (tag.EndsWith("!"))
                {
                    tag = tag.Remove(tag.Length - 1);
				string replacement= EventDictionary.TagDictionary[tag]+value+' ';
				parsedDesc = parsedDesc.Insert(i, replacement);
				i += (ushort)replacement.Length;
                }
                else
                {
                    
                }
				effects.Add(tag,value);
            }
        return parsedDesc;
    }
}
