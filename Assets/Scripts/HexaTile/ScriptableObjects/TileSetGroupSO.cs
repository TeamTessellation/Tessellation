using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// TileSet들의 Group
/// </summary>
[CreateAssetMenu(fileName = "Deck", menuName = "HexaSystem", order = 0)]
public class DeckSO : ScriptableObject
{
    public List<DeckData> Deck;

    public void SaveTileSet(List<TileSetData> tileSet)
    {
        Deck = tileSet.Select(x => { return new DeckData(x); }).ToList();
    }
}