﻿//using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Event //UNDONE
{
    // Название события
    public string Name;// {get; private set; }

    // Определяет, "хорошее" это событие или "плохое", необходимо для правильного влияния параметра удачи.
    public bool Good;// {get; private set; } { get; private set; }

	//TODO Возможно стоит вместо этого разделить события на два списка?
	// Определяет, может ли это событие произойти от действия.
    public bool ByAction;// {get; private set; } { get; private set; }

	// Определяет, может ли это событие произойти от таймера.
    public bool ByTime;// {get; private set; } { get; private set; }

	// Описание события, выводимое на экран.
    public string Description;// {get; private set; } { get; private set; }

	// Базовая вероятность возникновения события, на него будет влиять параметр удачи персонажа.
    public float Probability;// {get; private set; } { get; private set; }

    // TODO Коэффициент вероятности возникновения события при различных факторах, "1" - максимальное значение, "0" - событие не происходит в этом типе.
    public class TerrainCoefficients
    {
        public float ForestСoef;// {get; private set; } { get; private set; }
        public float RiverСoef;// {get; private set; } { get; private set; }
        // и т.п.
    }

    public TerrainCoefficients TerrainCoef;

    public class TimeCoefficients
    {
        public float DayСoef;// {get; private set; } { get; private set; }
        public float NightСoef;// {get; private set; } { get; private set; }
        // и т.п.
    }

    public TimeCoefficients TimeCoef;

    public class StateCoefficients
    {
        //TODO
    }

    public StateCoefficients StateCoef;

    // TODO Реакции на событие.
    public class Reaction
    {
        public string Description;// {get; private set; } { get; private set; }
        //TODO
        // и т.п.

        public string ResultDescription;// {get; private set; } { get; private set; }
        public float VitalityEffect;
    }

    public List<Reaction> Reactions;

        //public struct Result
        //{
        //    
        //}
        // TODO Результаты события.
        //public List<Result> Results;

        public Event(string name, bool good, bool byAction, bool byTime, string description, float probability, TerrainCoefficients terrainCoef, TimeCoefficients timeCoef, StateCoefficients stateCoef, List<Reaction> reactions)
	{
        Name = name;
		ByAction=byAction;
		ByTime=byTime;
		Description = description;
		Probability = probability;
		Good = good;
        TerrainCoef = terrainCoef;
        TimeCoef = timeCoef;
        StateCoef = stateCoef;
        Reactions = reactions;
	}
}