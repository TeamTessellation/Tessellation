using Cysharp.Threading.Tasks;

public class TileOptionGold : TileOptionBase
{
    public override async UniTask OnTilePlaced(Tile tile) => await ApplyScoreRule(eTileEventType.Place, tile);
    public override async UniTask OnLineCleared(Tile tile) => await ApplyScoreRule(eTileEventType.LineClear, tile);
    public override async UniTask OnTileBurst(Tile tile) => await ApplyScoreRule(eTileEventType.Burst, tile);
}
