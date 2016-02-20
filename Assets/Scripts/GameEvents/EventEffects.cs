using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventEffects : MonoBehaviour
{
    delegate void EffectApplier();
	delegate void ParamEffectApplier(float value);
	static Dictionary<string, EffectApplier> Effects;
	static Dictionary<string, ParamEffectApplier> ParamEffects;

    void Start()
    {
        Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
		World world = GameObject.FindWithTag("World").GetComponent<World>();
        Effects = new Dictionary<string, EffectApplier>{
			{"enemy",world.EnemyAttack}
		};
		ParamEffects = new Dictionary<string, ParamEffectApplier>{
			{"damage",player.TakeDamage},
			{"heal",player.TakeHeal},
		};
    }

    public static void ApplyEffect(string tag)
    {
        Effects[tag]();
    }

	public static void ApplyEffect(string tag, float value)
	{
		ParamEffects[tag](value);
	}
}
