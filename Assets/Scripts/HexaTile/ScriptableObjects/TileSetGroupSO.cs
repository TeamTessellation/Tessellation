using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// TileSetµÈ¿« Group
/// </summary>
[CreateAssetMenu(fileName = "TileSet", menuName = "TileSet/TileSetGroup", order = 0)]
public class TileSetGroupSO : ScriptableObject
{
    public List<GroupTileSetData> Group { get; private set; }

    public void SaveTileSet(List<TileSetData> tileSet)
    {
        Group = tileSet.Select(x => { return new GroupTileSetData(x); }).ToList();
    }
}