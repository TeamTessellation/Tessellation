using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using static Field;

public class LineClearHandler
{
    public List<Line> CheckLineClear(List<Tile> newTiles)
    {
        var clearLines = new List<Line>();
        if (newTiles == null || newTiles.Count == 0)
            return clearLines;

        var checkedLines = new HashSet<(Axis axis, int number)>();

        foreach (Tile tile in newTiles.Where(IsStillPlaced))
        {
            CheckAxis(Axis.X, tile.Coor.Pos3D.y, tile.Coor, checkedLines, clearLines);
            CheckAxis(Axis.Y, tile.Coor.Pos3D.x, tile.Coor, checkedLines, clearLines);
            CheckAxis(Axis.Z, tile.Coor.Pos3D.z, tile.Coor, checkedLines, clearLines);
        }

        return clearLines;
    }

    private static bool IsStillPlaced(Tile tile)
    {
        if (tile == null || !Field.Instance.CheckAbleCoor(tile.Coor))
            return false;

        return Field.Instance.GetTile(tile.Coor) == tile;
    }

    private void CheckAxis(
        Axis axis,
        int number,
        Coordinate coordinate,
        HashSet<(Axis axis, int number)> checkedLines,
        List<Line> clearLines)
    {
        if (!checkedLines.Add((axis, number)))
            return;

        if (TryGetCompleteLine(axis, coordinate, out Coordinate start))
            clearLines.Add(new Line(axis, start));
    }

    private bool TryGetCompleteLine(Axis axis, Coordinate coordinate, out Coordinate start)
    {
        GetDirections(axis, out Direction up, out Direction down);

        start = coordinate;
        while (Field.Instance.CheckAbleCoor(start + down))
            start += down;

        Coordinate current = start;
        while (Field.Instance.CheckAbleCoor(current))
        {
            if (!Field.Instance.ClearAble(current))
                return false;
            current += up;
        }

        return true;
    }

    public List<Tile> GetTilesFromLine(Line line)
    {
        GetDirections(line.Axis, out Direction up, out _);
        var tiles = new List<Tile>();
        Coordinate current = line.Start;

        while (Field.Instance.CheckAbleCoor(current))
        {
            Tile tile = Field.Instance.GetTile(current);
            if (tile != null)
                tiles.Add(tile);
            current += up;
        }

        return tiles;
    }

    public List<Tile> GetTilesFromLines(List<Line> lines)
    {
        return lines
            .SelectMany(GetTilesFromLine)
            .Where(tile => tile != null)
            .Distinct()
            .OrderBy(tile => tile.Coor.Pos.x)
            .ThenBy(tile => tile.Coor.Pos.y)
            .ToList();
    }

    public async UniTask RemoveTilesAsync(IEnumerable<Tile> tiles, float interval = 0.1f)
    {
        var uniqueTiles = tiles
            .Where(tile => tile != null)
            .GroupBy(tile => tile.Coor)
            .Select(group => group.First())
            .OrderBy(tile => tile.Coor.Pos.x)
            .ThenBy(tile => tile.Coor.Pos.y)
            .ToList();

        var tasks = new List<UniTask>(uniqueTiles.Count);
        for (int i = 0; i < uniqueTiles.Count; i++)
        {
            Tile tile = uniqueTiles[i];
            tasks.Add(RemoveWithDelay(tile, interval * i, Mathf.Min(5f, 1f + i * 0.15f)));
        }

        await UniTask.WhenAll(tasks);
    }

    private static async UniTask RemoveWithDelay(Tile tile, float delay, float pitch)
    {
        if (delay > 0f)
            await UniTask.WaitForSeconds(delay);

        if (tile != null && Field.Instance.CheckAbleCoor(tile.Coor) && Field.Instance.GetTile(tile.Coor) == tile)
            await Field.Instance.SafeRemoveTile(tile.Coor, sfx_pitch: pitch);
    }

    private static void GetDirections(Axis axis, out Direction up, out Direction down)
    {
        switch (axis)
        {
            case Axis.Y:
                up = Direction.RD;
                down = Direction.LU;
                break;
            case Axis.Z:
                up = Direction.RU;
                down = Direction.LD;
                break;
            default:
                up = Direction.R;
                down = Direction.L;
                break;
        }
    }
}
