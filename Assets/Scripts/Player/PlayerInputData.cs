using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Player
{
    public class PlayerInputData
    {
        public enum InputType
        {
            TilePlace,
            UseItem,
        }
        
        public InputType Type { get; }
        public List<Tile> PlacedTile { get; }
        // public Item UsedItem { get; set;}
        
        public PlayerInputData(List<Tile> tiles)
        {
            Type = InputType.TilePlace;
            PlacedTile = tiles;
        }
        
    }
}