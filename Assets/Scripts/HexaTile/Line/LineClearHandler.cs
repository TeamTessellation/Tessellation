using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using static Field;

public class LineClearHandler : MonoBehaviour
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

    private async UniTask ClearLineAsync(Line line, float interval = 1f)
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

        Field.Instance.SafeRemoveTile(line.Start);
        await UniTask.WaitForSeconds(interval);
        while (Field.Instance.CheckAbleCoor(upCorrect) || Field.Instance.CheckAbleCoor(downCorrect))
        {
            upCorrect += up;
            downCorrect += down;
            await UniTask.WaitForSeconds(interval);
            Field.Instance.SafeRemoveTile(upCorrect);
            Field.Instance.SafeRemoveTile(downCorrect);
        }
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

    private void EndLineClear(Line line)
    {

    }

    private void EndAllLineClear(List<Line> line)
    {
        Debug.Log("Line Å¬¸®¾î");
    }
}
