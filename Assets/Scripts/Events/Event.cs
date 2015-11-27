using UnityEngine;
using System.Collections;

public class Event //UNDONE
{	
	// Описание события, выводимое на экран.
	string Description;

	// TODO Базовые возможности успеха реакции на событие, "0" - отсутствие такого варианта.
	float RunAwayChance;
	// и т.п.

	// Базовая вероятность возникновения события, на него будет влиять параметр удачи персонажа.
	float Probability;
	// Определяет, "хорошее" это событие или "плохое", необходимо для правильного влияния параметра удачи.
	bool Good;

	// TODO Коэффициент вероятности возникновения события в типе местности, "0" - событие не происходит в этом типе.
	float ForestProb;
	float RiverProb;
	// и т.п.

	// TODO Результаты события.
	string ResultDescription;
	float HealthEffect;
	float StaminaEffect;
	// и т.п.

	public Event (string description, float runAwayChance, float probability, bool good, float forestProb, float riverProb, string resultDescription, float healthEffect, float staminaEffect)
	{
		Description = description;
		RunAwayChance = runAwayChance;
		Probability = probability;
		Good = good;
		ForestProb = forestProb;
		RiverProb = riverProb;
		ResultDescription = resultDescription;
		HealthEffect = healthEffect;
		StaminaEffect = staminaEffect;
	}
}
