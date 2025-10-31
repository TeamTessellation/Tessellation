public class TileData
{
    /// <summary>
    /// 옵션
    /// </summary>
    public TileOption Option = TileOption.Defaut;
    public float Scale = 1f;

    public TileData(TileOption option, float scale)
    {
        Option = option;
        Scale = scale;
    }

    public TileData()
    {
        Option = TileOption.Defaut;
        Scale = 1f;
    }
}
