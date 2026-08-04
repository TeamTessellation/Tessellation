using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ExecEvents;
using Player;
using Sound;
using Stage;
using UnityEngine;

[Serializable]
public readonly struct TileEventRecord
{
    public Coordinate Coordinate { get; }
    public TileOption Option { get; }
    public int TileScore { get; }

    public TileEventRecord(Tile tile)
    {
        Coordinate = tile.Coor;
        Option = tile.Data.Option;
        TileScore = tile.Data.Score;
    }
}

public class TurnResultInfo : ExecEventArgs<TurnResultInfo>
{
    public readonly List<TileEventRecord> PlacedTiles = new();
    public readonly List<TileEventRecord> RemovedTiles = new();
    public readonly List<TileEventRecord> BurstTiles = new();
    public readonly List<TileEventRecord> ClearedTiles = new();
    public int ClearedLineCount;

    public override void Clear()
    {
        base.Clear();
        PlacedTiles.Clear();
        RemovedTiles.Clear();
        BurstTiles.Clear();
        ClearedTiles.Clear();
        ClearedLineCount = 0;
    }
}

public enum eTileEventType
{
    Place,
    Remove,
    Burst,
    LineClear,
}

public class TilePlaceHandler : MonoBehaviour, IPlayerInputHandler
{
    [Header("Settings")]
    [SerializeField] private float tileRemoveInterval = 0.1f;

    public event Func<TurnResultInfo, UniTask> OnTilePlacedAsync;
    public event Func<TurnResultInfo, UniTask> OnLineClearedAsync;
    public event Func<TurnResultInfo, UniTask> OnTileRemovedAsync;
    public event Func<TurnResultInfo, UniTask> OnTileBurstAsync;
    public event Func<TurnResultInfo, UniTask> OnTurnProcessedAsync;

    public BombRules BombRules { get; } = new();

    private TurnResultInfo _turnResultInfo;

    public async UniTask HandlePlayerInput(PlayerInputData inputData, CancellationToken token)
    {
        if (inputData.Type == PlayerInputData.InputType.TilePlace)
            await FirstTilePlaced(inputData.PlacedTile, token);
    }

    public async UniTask FirstTilePlaced(List<Tile> tiles, CancellationToken token)
    {
        _turnResultInfo = TurnResultInfo.Get();
        try
        {
            List<Tile> placedTiles = (tiles ?? new List<Tile>())
                .Where(tile => tile != null)
                .Distinct()
                .ToList();
            foreach (Tile tile in placedTiles)
            {
                _turnResultInfo.PlacedTiles.Add(new TileEventRecord(tile));
                await tile.TileOptionBase.OnTilePlaced(tile);
            }

            await InvokeTileEventAsync(OnTilePlacedAsync, _turnResultInfo, token);
            await ScoreManager.Instance.FinalizeScore();

            if (BombRules.ExplodesImmediately)
            {
                List<Coordinate> immediateBombs = placedTiles
                    .Where(IsStillPlaced)
                    .Where(tile => tile.Data.Option == TileOption.Boom)
                    .Select(tile => tile.Coor)
                    .ToList();

                if (immediateBombs.Count > 0)
                    await ProcessExplosionAsync(immediateBombs, token);
            }

            List<Tile> survivingPlacedTiles = placedTiles.Where(IsStillPlaced).ToList();
            var lineHandler = new LineClearHandler();
            List<Field.Line> lines = lineHandler.CheckLineClear(survivingPlacedTiles);
            if (lines.Count > 0)
                await ProcessLinesAsync(lines, token);

            await InvokeTileEventAsync(OnTurnProcessedAsync, _turnResultInfo, token);
        }
        finally
        {
            _turnResultInfo.Dispose();
            _turnResultInfo = null;
        }
    }

    private async UniTask ProcessExplosionAsync(IEnumerable<Coordinate> seedBombs, CancellationToken token)
    {
        Dictionary<Coordinate, Tile> fieldTiles = SnapshotFieldTiles();
        ExplosionResolution resolution = ExplosionResolver.Resolve(
            fieldTiles.Keys,
            fieldTiles.Where(pair => pair.Value.Data.Option == TileOption.Boom).Select(pair => pair.Key),
            seedBombs,
            BombRules.ExplosionRadius,
            BombRules.Chains);

        List<Tile> targets = resolution.DestroyedCoordinates
            .Where(fieldTiles.ContainsKey)
            .OrderBy(coordinate => coordinate.Pos.x)
            .ThenBy(coordinate => coordinate.Pos.y)
            .Select(coordinate => fieldTiles[coordinate])
            .Distinct()
            .ToList();

        foreach (Tile tile in targets)
        {
            _turnResultInfo.BurstTiles.Add(new TileEventRecord(tile));
            await tile.TileOptionBase.OnTileBurst(tile);
        }

        PlayBombSounds(resolution.DetonatedBombCoordinates.Count);
        await new LineClearHandler().RemoveTilesAsync(targets, tileRemoveInterval);
        await InvokeTileEventAsync(OnTileBurstAsync, _turnResultInfo, token);
        await ScoreManager.Instance.FinalizeScore();
    }

    private async UniTask ProcessLinesAsync(List<Field.Line> lines, CancellationToken token)
    {
        var lineHandler = new LineClearHandler();
        List<Tile> lineTiles = lineHandler.GetTilesFromLines(lines);
        var lineCoordinates = new HashSet<Coordinate>(lineTiles.Select(tile => tile.Coor));
        Dictionary<Coordinate, Tile> fieldTiles = SnapshotFieldTiles();

        List<Coordinate> lineBombs = lineTiles
            .Where(tile => tile.Data.Option == TileOption.Boom)
            .Select(tile => tile.Coor)
            .ToList();

        ExplosionResolution explosion = ExplosionResolver.Resolve(
            fieldTiles.Keys,
            fieldTiles.Where(pair => pair.Value.Data.Option == TileOption.Boom).Select(pair => pair.Key),
            lineBombs,
            BombRules.ExplosionRadius,
            BombRules.Chains);

        var allCoordinates = new HashSet<Coordinate>(lineCoordinates);
        allCoordinates.UnionWith(explosion.DestroyedCoordinates);

        List<Tile> burstOnlyTiles = allCoordinates
            .Where(coordinate => !lineCoordinates.Contains(coordinate) && fieldTiles.ContainsKey(coordinate))
            .OrderBy(coordinate => coordinate.Pos.x)
            .ThenBy(coordinate => coordinate.Pos.y)
            .Select(coordinate => fieldTiles[coordinate])
            .Distinct()
            .ToList();

        int doubleTileCount = lineTiles.Count(tile =>
            TileScoreRules.Get(eTileEventType.LineClear, tile.Data.Option).AppliesDoubleMultiplier);
        float phaseMultiplier = TileScoreRules.CalculateLinePhaseMultiplier(
            lines.Count,
            doubleTileCount,
            ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseLineClearMultiple],
            ScoreManager.Instance.ScoreValues[ScoreManager.ScoreValueType.BaseMultipleTileValue]);
        if (!Mathf.Approximately(phaseMultiplier, 1f))
            ScoreManager.Instance.MultiplyMultiplier(phaseMultiplier);

        foreach (Tile tile in lineTiles)
        {
            _turnResultInfo.ClearedTiles.Add(new TileEventRecord(tile));
            await tile.TileOptionBase.OnLineCleared(tile);
        }

        foreach (Tile tile in burstOnlyTiles)
        {
            _turnResultInfo.BurstTiles.Add(new TileEventRecord(tile));
            await tile.TileOptionBase.OnTileBurst(tile);
        }

        _turnResultInfo.ClearedLineCount += lines.Count;
        PlayerStatus.Current.StageClearedLines += lines.Count;

        PlayBombSounds(explosion.DetonatedBombCoordinates.Count);
        List<Tile> allTargets = allCoordinates
            .Where(fieldTiles.ContainsKey)
            .OrderBy(coordinate => coordinate.Pos.x)
            .ThenBy(coordinate => coordinate.Pos.y)
            .Select(coordinate => fieldTiles[coordinate])
            .Distinct()
            .ToList();
        await lineHandler.RemoveTilesAsync(allTargets, tileRemoveInterval);

        // A line-triggered explosion is one scoring phase, so generic abilities activate once.
        await InvokeTileEventAsync(OnLineClearedAsync, _turnResultInfo, token);
        await ScoreManager.Instance.FinalizeScore();
    }

    private static bool IsStillPlaced(Tile tile)
    {
        return tile != null && Field.Instance.TryGetTile(tile.Coor, out Tile placedTile) && placedTile == tile;
    }

    private static Dictionary<Coordinate, Tile> SnapshotFieldTiles()
    {
        return Field.Instance
            .Where(cell => !cell.IsLock && !cell.IsEmpty)
            .ToDictionary(cell => cell.Coor, cell => cell.Tile);
    }

    private static void PlayBombSounds(int count)
    {
        for (int i = 0; i < count; i++)
            SoundManager.Instance.PlaySfx(SoundReference.TileBomb, pitch: Mathf.Min(2f, 1f + i * 0.1f));
    }

    private async UniTask InvokeTileEventAsync(
        Func<TurnResultInfo, UniTask> eventDelegate,
        TurnResultInfo info,
        CancellationToken token)
    {
        info.BreakChain = false;
        await ExecEventBus<TurnResultInfo>.InvokeMerged(info, token);
        if (eventDelegate == null)
            return;

        foreach (Func<TurnResultInfo, UniTask> handler in eventDelegate.GetInvocationList()
                     .Cast<Func<TurnResultInfo, UniTask>>())
        {
            await handler(info).AttachExternalCancellation(token);
        }
    }
}
