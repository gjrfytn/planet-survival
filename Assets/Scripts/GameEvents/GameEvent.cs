using System.Collections.Generic;
using System.IO;

public class GameEvent : IBinaryReadableWriteable
{
    // Название события
    public string Name { get; private set; }

    // Определяет, яляется ли событие сюжетным.
    public bool StoryEvent { get; private set; }

    // Описание события, выводимое на экран.
    public string Description { get; private set; }

    // Базовая вероятность возникновения события.
    public float BaseProbability { get; private set; }

    // Текущая (переменная) вероятность возникновения события.
    public float Probability;

    // TODO Вес события при различных факторах.

    public Dictionary<TerrainType, sbyte> TerrainWeights { get; private set; }

    public class TimeWeights
    {
        public readonly sbyte Day;
        public readonly sbyte Night;
        // и т.п.

        public TimeWeights(sbyte day, sbyte night)
        {
            Day = day;
            Night = night;
        }
    }

    public TimeWeights TimeWeight { get; private set; }

    public class StateWeights
    {
        //TODO
    }

    public StateWeights StateWeight { get; private set; }

    // TODO Реакции на событие.
    public class Reaction
    {
        public class Result
        {
            public readonly string Description;
            public readonly float Probability;

            public Result(string description, float probability)
            {
                Description = description;
                Probability = probability;
            }
        }
        public readonly string Description;
        //TODO
        // и т.п.
        public readonly List<Result> Results;

        public Reaction(string description, List<Result> results)
        {
            Description = description;
            Results = results;
        }
    }

    public List<Reaction> Reactions { get; private set; }

    public GameEvent()
    { }

    public GameEvent(string name, bool storyEvent, string description, float probability, Dictionary<TerrainType, sbyte> terrainWeights, TimeWeights timeWeight, StateWeights stateWeight, List<Reaction> reactions)
    {
        Name = name;
        Description = description;
        BaseProbability = Probability = probability;
        StoryEvent = storyEvent;
        TerrainWeights = terrainWeights;
        TimeWeight = timeWeight;
        StateWeight = stateWeight;
        Reactions = reactions;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Name);
        writer.Write(StoryEvent);
        writer.Write(Description);
        writer.Write(Probability);

        writer.Write((byte)(System.Enum.GetNames(typeof(TerrainType)).Length - 1));
        foreach (sbyte weight in TerrainWeights.Values)
            writer.Write(weight);

        writer.Write(TimeWeight.Day);
        writer.Write(TimeWeight.Night);

        //

        writer.Write((byte)Reactions.Count);
        foreach (GameEvent.Reaction reaction in Reactions)
        {
            writer.Write(reaction.Description);
            writer.Write((byte)reaction.Results.Count);
            foreach (GameEvent.Reaction.Result result in reaction.Results)
            {
                writer.Write(result.Description);
                writer.Write(result.Probability);
            }
        }
    }

    public void Read(SymBinaryReader reader)
    {
        Name = reader.ReadString();
        StoryEvent = reader.ReadBoolean();
        Description = reader.ReadString();
        BaseProbability = Probability = reader.ReadSingle();

        byte terrainsCount = (byte)(System.Enum.GetNames(typeof(TerrainType)).Length - 1);
        TerrainWeights = new Dictionary<TerrainType, sbyte>(terrainsCount);
        byte weightsCount = reader.ReadByte();
        for (byte i = 0; i < weightsCount; ++i)
            TerrainWeights.Add((TerrainType)(1 << i), reader.ReadSByte());
        for (byte i = weightsCount; i < terrainsCount; ++i)
            TerrainWeights.Add((TerrainType)(1 << i), 0);

        TimeWeight = new GameEvent.TimeWeights
        (
             reader.ReadSByte(),
             reader.ReadSByte()
        );

        StateWeight = new GameEvent.StateWeights();
        //

        byte reactionCount = reader.ReadByte();
        Reactions = new List<GameEvent.Reaction>(reactionCount);
        for (byte i = 0; i < reactionCount; ++i)
        {
            string reacDesc = reader.ReadString();

            byte resultCount = reader.ReadByte();
            List<GameEvent.Reaction.Result> results = new List<GameEvent.Reaction.Result>(resultCount);
            for (byte j = 0; j < resultCount; ++j)
                results.Add(new GameEvent.Reaction.Result
                (
                     reader.ReadString(),
                    reader.ReadSingle()
                ));

            Reactions.Add(new GameEvent.Reaction(reacDesc, results));
        }
    }
}
