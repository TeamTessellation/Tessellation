using System;

[AttributeUsage(AttributeTargets.Class)]
public class PoolSizeAttribute : Attribute
{
    public int Size { get; }

    public PoolSizeAttribute(int size)
    {
        Size = size;
    }
}
