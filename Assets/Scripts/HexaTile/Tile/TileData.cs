[System.Serializable]
public struct TileData
{
    /// <summary>
    /// 옵션
    /// </summary>
    public TileOption Option;
    public int Score;
    public float Scale;

    public TileData(TileOption option, float scale)
    {
        Option = option;
        Scale = scale;
        Score = 1;
    }
}
