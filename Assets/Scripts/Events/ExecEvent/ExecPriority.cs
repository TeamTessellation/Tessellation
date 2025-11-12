using System;

namespace ExecEvents
{
    public enum ExecPriority
    {
        First = Int32.MinValue,
        
        
        Normal = 0,
        
        
        UIDefault = 1000,
        
        Last = Int32.MaxValue // UI등등
    }
}