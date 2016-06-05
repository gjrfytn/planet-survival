using UnityEngine;
using System.Collections.Generic;

public class EventEffects : MonoBehaviour
{
    static Dictionary<string, EffectApplier> Effects;

    void Start()
    {
        Player player = UnityEngine.GameObject.FindWithTag("Player").GetComponent<Player>();
        World world = UnityEngine.GameObject.FindWithTag("World").GetComponent<World>();
        Effects = new Dictionary<string, EffectApplier>
		{
			{"enemy",new EffectApplier(world.EnemyAttack)},
			{"damage",new ByteAndBoolEffectApplier(player.TakeDamage)},
			{"heal",new ByteEffectApplier(player.TakeHeal)}
		};
    }

    public static void ApplyEffect(string tag)//C#6.0 EBD
    {
        Effects[tag].Execute();
    }

    public static void ApplyEffect(string tag, byte value)//C#6.0 EBD
    {
        (Effects[tag] as ByteEffectApplier).Execute(value);
    }

    public static void ApplyEffect(string tag, byte value1, bool value2)//C#6.0 EBD
    {
        (Effects[tag] as ByteAndBoolEffectApplier).Execute(value1, value2);
    }
}
