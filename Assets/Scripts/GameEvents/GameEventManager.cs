using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class GameEventManager : MonoBehaviour
{
    [SerializeField]
    GameObject EventPanel;
    [SerializeField]
    GameObject ReactionButtonPanel;
    [SerializeField]
    GameObject Btn;
    const string EventsFilename = "events.edb";
    List<GameEvent> Events = new List<GameEvent>();
    List<GameObject> ReactionButtons = new List<GameObject>();
    GameEvent CurrentEvent;

    void OnEnable()
    {
        EventManager.HourPassed += CallEvent;
    }

    void OnDisable()
    {
        EventManager.HourPassed -= CallEvent;
    }

    void Start()
    {
        LoadEventsFromFile(Path.Combine(Application.streamingAssetsPath, EventsFilename)); //UNDONE Не будет работать на Android?
    }

    void CallEvent()
    {
        //CollectFactors();
        TerrainType terrain = GameObject.FindWithTag("World").GetComponent<World>().GetHexTerrain(GameObject.FindWithTag("Player").GetComponent<Player>().GlobalPos);

        List<GameEvent> possibleEvents = new List<GameEvent>(Events);//List<GameEvent> possibleEvents = Events.Where(e => true == true).ToList(); //TODO Фильтрация
        short weight = -100; //TODO
        GameEvent evnt = null;
        foreach (GameEvent e in possibleEvents)
        {
            short buf = CalculateTerrainWeight(e, terrain)/*+...*/;
            if (buf > weight)
            {
                weight = buf;
                evnt = e;
            }
        }
        if (Random.value < evnt.Probability)
        {
            evnt.Probability = evnt.BaseProbability;
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
            evnt.Probability += 0.01f;
    }

    void CollectFactors()
    {

    }

    short CalculateTerrainWeight(GameEvent evnt, TerrainType terrain)
    {
        short weight = 0;
        byte terrainsCount = (byte)(System.Enum.GetNames(typeof(TerrainType)).Length - 1);
        for (byte i = 0; i < terrainsCount; ++i)
        {
            TerrainType t = (TerrainType)(1 << i);
            if ((t & terrain) != TerrainType.NONE)
                weight += evnt.TerrainWeights[t];
        }
        return weight;
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
        Dictionary<string, byte?> effects;
        Debug.Log(ParseResultDescription(result.Description, out effects));
        foreach (KeyValuePair<string, byte?> effect in effects)
        {
            if (effect.Value.HasValue)
                EventEffects.ApplyEffect(effect.Key, effect.Value.Value);
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
            using (SymBinaryReader reader = new SymBinaryReader(File.Open(filename, FileMode.Open)))
            {
                while (reader.PeekChar() != -1)
                {
                    GameEvent buf = new GameEvent();
                    buf.Read(reader);
                    Events.Add(buf);
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
                    string replacement = EventDictionary.TagDictionary[tag] + ' ';
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

    string ParseResultDescription(string description, out Dictionary<string, byte?> effects)
    {
        string parsedDesc = description;
        effects = new Dictionary<string, byte?>();
        for (ushort i = 0; i < parsedDesc.Length; ++i)
            if (parsedDesc[i] == '<')
            {
                string tag = string.Empty;
                byte? value = null;
                byte j;
                for (j = 1; parsedDesc[i + j] != '>'; ++j)
                {
                    if (parsedDesc[i + j] == '=')
                    {
                        string strValue = string.Empty;
                        for (++j; parsedDesc[i + j] != '>'; ++j)
                        {
                            if (parsedDesc[i + j] == '!')
                            {
                                tag += parsedDesc[i + j];
                                ++j;
                                break;
                            }
                            strValue += parsedDesc[i + j];
                        }
                        value = byte.Parse(strValue);
                        break;
                    }
                    tag += parsedDesc[i + j];
                }
                parsedDesc = parsedDesc.Remove(i, j + 1);
                if (tag.EndsWith("!"))
                {
                    tag = tag.Remove(tag.Length - 1);
                    string replacement = EventDictionary.TagDictionary[tag] + value + ' ';
                    parsedDesc = parsedDesc.Insert(i, replacement);
                    i += (ushort)replacement.Length;
                }
                else
                {

                }
                effects.Add(tag, value);
            }
        return parsedDesc;
    }
}
