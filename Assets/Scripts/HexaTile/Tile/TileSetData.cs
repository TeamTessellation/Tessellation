using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class TileSetData
{
    public List<(Coordinate coor, TileData tileData)> Data;
}

[System.Serializable]
public class GroupTileSetData
{
    public TileSetData TileSet;
    public int Count;
    public string Id;

    public GroupTileSetData(TileSetData tileSet)
    {
        TileSet = tileSet;
        Count = 1;
        Id = "";
    }
}