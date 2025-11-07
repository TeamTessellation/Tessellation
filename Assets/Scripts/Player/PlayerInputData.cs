using System.Collections.Generic;
using NUnit.Framework;
using Stage;
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
        public List<Field.Line> ClearLines { get; }
        
        /// <summary>
        /// 플레이어 액션 전에 발생한 이벤트 인자.
        /// 해당 턴 루프가 끝나면 유효하지 않게 됩니다.
        /// </summary>
        public BeforePlayerActionEventArgs BeforeActionEventArgs { get; set; }
        // public Item UsedItem { get; set;}
        
        public PlayerInputData(List<Tile> tiles)
        {
            Type = InputType.TilePlace;
            PlacedTile = tiles;
        }
        
    }
}