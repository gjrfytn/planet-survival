using UnityEngine;
using System.Collections.Generic;

public class EventEffects : MonoBehaviour
{
    class EffectApplier
    {
        public delegate void VoidDelegate();
        VoidDelegate Applier;
        public EffectApplier(VoidDelegate applier)
        {
            Applier = applier;
        }

        public void Execute()//C#6.0 EBD
        {
            Applier();
        }
    }

    sealed class ByteEffectApplier : EffectApplier
    {
        public delegate void ByteDelegate(byte f);
        ByteDelegate Applier;
        public ByteEffectApplier(ByteDelegate applier)
            : base(delegate { })
        {
            Applier = applier;
        }

        public void Execute(byte f)//C#6.0 EBD
        {
            Applier(f);
        }
    }

    sealed class ByteAndBoolEffectApplier : EffectApplier
    {
        public delegate void ByteAndBoolDelegate(byte f, bool b);
        ByteAndBoolDelegate Applier;
        public ByteAndBoolEffectApplier(ByteAndBoolDelegate applier)
            : base(delegate { })
        {
            Applier = applier;
        }

        public void Execute(byte f, bool b)//C#6.0 EBD
        {
            Applier(f, b);
        }
    }

    static Dictionary<string, EffectApplier> Effects;

    void Start()
    {
        Player player = UnityEngine.GameObject.FindWithTag("Player").GetComponent<Player>();
        World world = UnityEngine.GameObject.FindWithTag("World").GetComponent<WorldWrapper>().World;
        Effects = new Dictionary<string, EffectApplier>
		{
			{"enemy",new EffectApplier(world.EnemyAttack)},
			{"damage",new ByteAndBoolEffectApplier(player.TakeDamage)},
			{"heal",new ByteEffectApplier(player.TakeHeal)},
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
