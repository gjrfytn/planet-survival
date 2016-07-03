
public class EffectApplier
{
    public delegate void VoidDelegate();
    VoidDelegate Applier;

    public EffectApplier(VoidDelegate applier)
    {
        Applier = applier;
    }

    public virtual void Execute()//C#6.0 EBD
    {
        Applier();
    }
}

public sealed class ByteEffectApplier : EffectApplier
{
    public delegate void ByteDelegate(byte b);

    ByteDelegate Applier;
    //byte Param;

    public ByteEffectApplier(ByteDelegate applier)
        : base(delegate { })
    {
        Applier = applier;
    }

    /*public ByteEffectApplier(ByteDelegate applier,byte param)
        : base(delegate { })
    {
        Applier = applier;
        Param=param;
    }

    public override void Execute()//C#6.0 EBD
    {
        Applier(Param);
    }*/

    public void Execute(byte param)//C#6.0 EBD
    {
        Applier(param);
    }
}

public sealed class ByteAnd2BoolEffectApplier : EffectApplier
{
    public delegate void ByteAnd2BoolDelegate(byte f, bool b1, bool b2);

    ByteAnd2BoolDelegate Applier;
    //byte Param1;
    //bool Param2;

    public ByteAnd2BoolEffectApplier(ByteAnd2BoolDelegate applier)
        : base(delegate { })
    {
        Applier = applier;
    }

    /*public ByteAndBoolEffectApplier(ByteAndBoolDelegate applier,byte param1, bool param2)
        : base(delegate { })
    {
        Applier = applier;
        Param1=param1;
        Param2=param2;
    }

    public override void Execute()//C#6.0 EBD
    {
        Applier(Param1, Param2);
    }*/

    public void Execute(byte param1, bool param2, bool param3)//C#6.0 EBD
    {
        Applier(param1, param2, param3);
    }
}
