using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventEffects : MonoBehaviour
{
	delegate void EffectApplier(float value);
	static Dictionary<string,EffectApplier> Effects;

	void Start()
	{
		Player player=GameObject.FindWithTag("Player").GetComponent<Player>();
		Effects=new Dictionary<string, EffectApplier>{
			{"damage",player.TakeDamage},
			{"heal",player.TakeHeal}
		};
	}

	public static void ApplyEffect(string tag,float value)
	{
		Effects[tag](value);
	}
}
