using System.Collections;
using System.Collections.Generic;

public class GameEvent //UNDONE
{
    // Название события
    public string Name;// {get; private set; }

    // Определяет, яляется ли событие сюжетным.
    public bool StoryEvent;// {get; private set; }

    // Описание события, выводимое на экран.
    public string Description;// {get; private set; }

    // Базовая вероятность возникновения события.
    float BaseProbability_;
    public float BaseProbability
    {
        get
        {
            return BaseProbability_;
        }
        set
        {
            BaseProbability_ = Probability = value;
        }
    }

    // Текущая (переменная) вероятность возникновения события.
    public float Probability;// {get; private set; }

    // TODO Вес события при различных факторах.

    public Dictionary<TerrainType, sbyte> TerrainWeights;

    public class TimeWeights
    {
        public sbyte Day;// {get; private set; }
        public sbyte Night;// {get; private set; }
        // и т.п.
    }

    public TimeWeights TimeWeight;

    public class StateWeights
    {
        //TODO
    }

    public StateWeights StateWeight;

    // TODO Реакции на событие.
    public class Reaction
    {
        public class Result
        {
            public string Description;
            public float Probability;
        }
        public string Description;// {get; private set; }
        //TODO
        // и т.п.
        public List<Result> Results;
    }

    public List<Reaction> Reactions;

    public GameEvent(string name, bool storyEvent, string description, float probability, Dictionary<TerrainType, sbyte> terrainWeights, TimeWeights timeWeight, StateWeights stateWeight, List<Reaction> reactions)
    {
        Name = name;
        Description = description;
        Probability = probability;
        StoryEvent = storyEvent;
        TerrainWeights = terrainWeights;
        TimeWeight = timeWeight;
        StateWeight = stateWeight;
        Reactions = reactions;
    }
}
