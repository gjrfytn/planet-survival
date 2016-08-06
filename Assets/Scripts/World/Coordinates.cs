
[System.Serializable]
public struct U16Vec2
{
    public ushort X;
    public ushort Y;

    public U16Vec2(ushort x, ushort y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(U16Vec2 op1, U16Vec2 op2)
    {
        return op1.X == op2.X && op1.Y == op2.Y;
    }

    public static bool operator !=(U16Vec2 op1, U16Vec2 op2)
    {
        return op1.X != op2.X || op1.Y != op2.Y;
    }

    public static explicit operator U16Vec2(S32Vec2 op)
    {
        return new U16Vec2((ushort)op.X, (ushort)op.Y);//TODO   -new !!!!!!!!!!!!!!!!!!!!!!!!!!!
    }

    public override bool Equals(System.Object obj)
    {
        if (obj is U16Vec2)
        {
            U16Vec2 p = (U16Vec2)obj;
            return X == p.X && Y == p.Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return new System.Collections.Generic.KeyValuePair<ushort, ushort>(X, Y).GetHashCode();
    }

    public override string ToString()
    {
        return '(' + X.ToString() + ", " + Y.ToString() + ')';
    }
}

[System.Serializable]
public struct S32Vec2
{
    public int X;
    public int Y;

    public S32Vec2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(S32Vec2 op1, S32Vec2 op2)
    {
        return op1.X == op2.X && op1.Y == op2.Y;
    }

    public static bool operator !=(S32Vec2 op1, S32Vec2 op2)
    {
        return op1.X != op2.X || op1.Y != op2.Y;
    }

    public static implicit operator S32Vec2(U16Vec2 op)
    {
        return new S32Vec2(op.X, op.Y); //TODO   -new !!!!!!!!!!!!!!!!!!!!!!!!!!!
    }

    public override bool Equals(System.Object obj)
    {
        if (obj is S32Vec2)
        {
            S32Vec2 p = (S32Vec2)obj;
            return X == p.X && Y == p.Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return new System.Collections.Generic.KeyValuePair<int, int>(X, Y).GetHashCode();
    }

    public override string ToString()
    {
        return '(' + X.ToString() + ", " + Y.ToString() + ')';
    }
}
