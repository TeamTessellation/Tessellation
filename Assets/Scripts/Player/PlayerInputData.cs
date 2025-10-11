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
        public Vector2 StartPosition { get; }
        public Vector2 EndPosition { get; }
        public InputType Type { get; }
        // public TileData PlacedTile { get; set;}
        // public Item UsedItem { get; set;}
        
        public PlayerInputData(Vector2 startPosition, Vector2 endPosition, InputType type)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Type = type;
        }
        
    }
}