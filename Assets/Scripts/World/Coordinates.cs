
[System.Serializable]
public struct LocalPos
{
    public ushort X;
    public ushort Y;

    public LocalPos(ushort x, ushort y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(LocalPos op1, LocalPos op2)
    {
        return op1.X == op2.X && op1.Y == op2.Y;
    }

    public static bool operator !=(LocalPos op1, LocalPos op2)
    {
        return op1.X != op2.X || op1.Y != op2.Y;
    }

    public static explicit operator LocalPos(GlobalPos op)
    {
        return new LocalPos((ushort)op.X, (ushort)op.Y);//TODO   -new !!!!!!!!!!!!!!!!!!!!!!!!!!!
    }

    public override bool Equals(System.Object obj)
    {
        if (obj is LocalPos)
        {
            LocalPos p = (LocalPos)obj;
            return X == p.X && Y == p.Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return new System.Collections.Generic.KeyValuePair<ushort, ushort>(X, Y).GetHashCode();
    }
}

[System.Serializable]
public struct GlobalPos
{
    public int X;
    public int Y;

    public GlobalPos(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(GlobalPos op1, GlobalPos op2)
    {
        return op1.X == op2.X && op1.Y == op2.Y;
    }

    public static bool operator !=(GlobalPos op1, GlobalPos op2)
    {
        return op1.X != op2.X || op1.Y != op2.Y;
    }

    public static implicit operator GlobalPos(LocalPos op)
    {
        return new GlobalPos(op.X, op.Y); //TODO   -new !!!!!!!!!!!!!!!!!!!!!!!!!!!
    }

    public override bool Equals(System.Object obj)
    {
        if (obj is GlobalPos)
        {
            GlobalPos p = (GlobalPos)obj;
            return X == p.X && Y == p.Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return new System.Collections.Generic.KeyValuePair<int, int>(X, Y).GetHashCode();
    }
}
