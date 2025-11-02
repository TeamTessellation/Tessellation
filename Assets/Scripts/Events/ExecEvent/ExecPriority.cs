using System;

namespace PriortyExecEvent
{
    public enum ExecPriority
    {
        VeryHigh = Int32.MinValue,
        
        
        Normal = 0,
        
        
        VeryLow = Int32.MaxValue // UI등등
    }
}