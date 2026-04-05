using Core;
using Cysharp.Threading.Tasks;

public class TileOptionBoom : TileOptionBase
{
    public override async UniTask OnTilePlaced(Tile tile)
    {
        int baseScore = (int)ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BasePlaceScore];
        int finalScore = ScoreManager.Instance.CalculateTileScore(eTileEventType.Place, tile, baseScore);
        ScoreManager.Instance.AddCurrentScore(finalScore);

        ShowScoreEffect(finalScore, tile);
    }

    public override async UniTask OnTileBurst(Tile tile)
    {
        await Field.Instance.SafeRemoveTile(tile.Coor);

        // 6방향 타일 한번에 추가 및 폭발
        var removeTasks = new UniTask[6];
        for (int i = 0; i <= (int)Direction.LU; i++)
        {
            Coordinate neighborCoor = tile.Coor + (Direction)i;

            if (Field.Instance.CheckAbleCoor(neighborCoor))
            {
                Tile neighborTile = Field.Instance.GetTile(neighborCoor);
                if (neighborTile != null)
                    await neighborTile.TileOptionBase.OnTileBurst(neighborTile);
            }

            removeTasks[i] = Field.Instance.SafeRemoveTile(neighborCoor);
        }
        await UniTask.WhenAll(removeTasks);
    }
}
