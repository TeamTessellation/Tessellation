using Cysharp.Threading.Tasks;
using System.Drawing;
using UnityEngine;

public abstract class TileOptionBase
{
    public virtual async UniTask OnTilePlaced(Tile tile)
    {

    }

    public virtual async UniTask OnLineCleared(Tile tile)
    {
    }

    public virtual async UniTask OnTileRemoved(Tile tile)
    {

    }

    public virtual async UniTask OnTileBurst(Tile tile)
    {

    }
}
