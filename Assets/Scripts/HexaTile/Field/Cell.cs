using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public GameObject BG { get; private set; }
    public Tile Tile { get; private set; }
    public bool IsEmpty => Tile == null;
    public Coordinate Coor { get; private set; }

    private Transform _cellRoot;

    // 최소 Cell 세팅
    public void Init(Coordinate coor, Transform bgRoot, Transform cellRoot)
    {
        Coor = coor;
        BG = Pool.Get("BGTile");
        BG.transform.SetParent(bgRoot);
        BG.transform.localPosition = coor.ToWorld();
        Tile = null;
        _cellRoot = cellRoot;
    }

    public void Remove()
    {
        UnSet();
        Pool.Return(BG);
    }

    public void SetCoor(Coordinate coor) => Coor = coor;

    /// <summary>
    /// 배치 된 Tile을 해제 & Pool에 반납
    /// </summary>
    public void UnSet()
    {
        if (!IsEmpty) Pool<Tile>.Return(Tile);
        Tile = null;
    }

    /// <summary>
    /// 해당 cell에 전달받은 Tile 배치
    /// </summary>
    /// <param name="tile">배치할 Tile</param>
    public void Set(Tile tile)
    {
        Tile = tile;
        tile.Coor = Coor;
        tile.transform.SetParent(_cellRoot, true);
        tile.gameObject.transform.position = tile.Coor.ToWorld();
    }
}
