using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class TileSetData
{
    public List<OffsetTileData> Data = new();
    public float Size;
    public float Offset;
}

[System.Serializable]
public class OffsetTileData
{
    public Coordinate Coor;
    public TileData TileData = new(TileOption.Defaut, 1);
}

[System.Serializable]
public class DeckData
{
    public TileSetData TileSet;
    public int Count;

    public DeckData(TileSetData tileSet)
    {
        TileSet = tileSet;
        Count = 1;
    }

    public DeckData()
    {
        TileSet = new();
        Count = 1;
    }
}