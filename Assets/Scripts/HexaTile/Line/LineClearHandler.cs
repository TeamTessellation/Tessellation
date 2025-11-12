using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Core;
using Player;
using UnityEngine;
using static Field;

public class LineClearHandler
{
    public List<Line> CheckLineClear(List<Tile> newTile)
    {
        HashSet<int> xCheckList = new();
        HashSet<int> yCheckList = new();
        HashSet<int> zCheckList = new();

        List<Line> clearLines = new();

        int upOrDown = UnityEngine.Random.Range(0, 2); // 0 up 1 down

        for (int i = 0; i < newTile.Count; i++)
        {
            if (!xCheckList.Contains(newTile[i].Coor.Pos3D.y))
            {
                CheckAxis(upOrDown, xCheckList, newTile[i].Coor.Pos3D.y, Axis.X, newTile[i], clearLines);
            }
            if (!yCheckList.Contains(newTile[i].Coor.Pos3D.x))
            {
                CheckAxis(upOrDown, yCheckList, newTile[i].Coor.Pos3D.x, Axis.Y, newTile[i], clearLines);
            }
            if (!zCheckList.Contains(newTile[i].Coor.Pos3D.z))
            {
                CheckAxis(upOrDown, zCheckList, newTile[i].Coor.Pos3D.z, Axis.Z, newTile[i], clearLines);
            }
        }

        return clearLines;
    }

    private void CheckAxis(int upOrDown, HashSet<int> checkList, int key, Axis axis, Tile tile, List<Line> clearLines)
    {
        checkList.Add(key);
        if (CheckLine(upOrDown, axis, tile.Coor, out Coordinate start))
            clearLines.Add(new(axis, start));
    }

    private bool CheckLine(int upOrDown, Axis axis, Coordinate start, out Coordinate result)
    {
        Direction up = Direction.R;
        Direction down = Direction.L;
        result = start;

        switch (axis)
        {
            case Axis.X:
                up = Direction.R;
                down = Direction.L;
                break;
            case Axis.Y:
                up = Direction.RD;
                down = Direction.LU;
                break;
            case Axis.Z:
                up = Direction.RU;
                down = Direction.LD;
                break;
        }

        Coordinate correctCoor = start;
        while (Field.Instance.CheckAbleCoor(correctCoor) && Field.Instance.GetTile(correctCoor) != null)
        {
            correctCoor += up;
        }
        if (correctCoor.CircleRadius <= Field.Instance.Size)
            return false;

        if (upOrDown == 0)
            result = correctCoor - up;

        correctCoor = start;
        while (Field.Instance.CheckAbleCoor(correctCoor) && Field.Instance.GetTile(correctCoor) != null)
        {
            correctCoor += down;
        }
        if (correctCoor.CircleRadius <= Field.Instance.Size)
            return false;

        if (upOrDown == 1)
            result = correctCoor - down;

        return true;
    }

    public async UniTask ClearLineAsync(Line line, float interval = 1f)
    {
        Direction up = Direction.R;
        Direction down = Direction.L;

        switch (line.Axis)
        {
            case Axis.X:
                up = Direction.R;
                down = Direction.L;
                break;
            case Axis.Y:
                up = Direction.RD;
                down = Direction.LU;
                break;
            case Axis.Z:
                up = Direction.RU;
                down = Direction.LD;
                break;
        }

        Coordinate upCorrect = line.Start;
        Coordinate downCorrect = line.Start;

        List<UniTask> allTask = new();

        await UniTask.WaitForSeconds(0.3f);

        allTask.Add(Field.Instance.SafeRemoveTile(line.Start));
        await UniTask.WaitForSeconds(interval);
        while (Field.Instance.CheckAbleCoor(upCorrect) || Field.Instance.CheckAbleCoor(downCorrect))
        {
            upCorrect += up;
            downCorrect += down;
            await UniTask.WaitForSeconds(interval);
            allTask.Add(Field.Instance.SafeRemoveTile(upCorrect));
            allTask.Add(Field.Instance.SafeRemoveTile(downCorrect));
        }

        await allTask;

        EndLineClear(line);
    }
    
    private async UniTask ClearLinesAsync(List<Line> line, float interval = 1f)
    {
        //UniTask[] tasks = new UniTask[line.Count];

        for (int i = 0; i < line.Count; i++)
        {
            await ClearLineAsync(line[i], interval);
        }
        //await UniTask.WhenAll(tasks);
        await UniTask.WaitForSeconds(interval);
        EndAllLineClear(line);
    }

    public List<Tile> GetTilesFromLine(Line line)
    {
        List<Tile> tiles = new List<Tile>();
        Direction up, down;
        
        // 축에 따른 방향 설정
        switch (line.Axis)
        {
            case Axis.X:
                up = Direction.R;
                down = Direction.L;
                break;
            case Axis.Y:
                up = Direction.RD;
                down = Direction.LU;
                break;
            case Axis.Z:
                up = Direction.RU;
                down = Direction.LD;
                break;
            default:
                return tiles; // 빈 리스트 반환
        }
        
        // 시작점 타일 추가
        Tile startTile = Field.Instance.GetTile(line.Start);
        if (startTile != null)
            tiles.Add(startTile);
        
        // 위쪽 방향 타일들 수집
        Coordinate upCoord = line.Start + up;
        while (Field.Instance.CheckAbleCoor(upCoord))
        {
            Tile tile = Field.Instance.GetTile(upCoord);
            if (tile != null)
                tiles.Add(tile);
            else
                break; // 빈 공간을 만나면 중단
            upCoord += up;
        }
        
        // 아래쪽 방향 타일들 수집
        Coordinate downCoord = line.Start + down;
        while (Field.Instance.CheckAbleCoor(downCoord))
        {
            Tile tile = Field.Instance.GetTile(downCoord);
            if (tile != null)
                tiles.Add(tile);
            else
                break; // 빈 공간을 만나면 중단
            downCoord += down;
        }
        
        return tiles;
    }
    
    public List<Tile> GetTilesFromLines(List<Line> lines)
    {
        HashSet<Tile> tileSet = new HashSet<Tile>();
        
        foreach (var line in lines)
        {
            List<Tile> lineTiles = GetTilesFromLine(line);
            foreach (var tile in lineTiles)
            {
                tileSet.Add(tile);
            }
        }
        
        return tileSet.ToList();
    }

    private void EndLineClear(Line line)
    {
        PlayerStatus status = GameManager.Instance.PlayerStatus;
        status.StageClearedLines += 1;
        // status.TotalClearedLines += 1; // 집계는 StageManager에서 처리
    }

    private void EndAllLineClear(List<Line> line)
    {
        Debug.Log("Line 클리어");
    }
}
