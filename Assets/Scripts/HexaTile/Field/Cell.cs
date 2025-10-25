using System.Collections.Generic;

public class Cell
{
    public Tile Tile { get; private set; }
    public bool IsEmpty => Tile == null;
    public Coordinate Coor { get; private set; }

    public void Init(Coordinate coor)
    {
        Coor = coor;
        Tile = null;
    }

    public void SetCoor(Coordinate coor) => Coor = coor;

    /// <summary>
    /// 배치 된 Tile을 해제
    /// </summary>
    public void UnSet() => Tile = null;

    /// <summary>
    /// 해당 cell에 Tile 배치
    /// </summary>
    /// <param name="tile">배치할 Tile</param>
    public void Set(Tile tile)
    {
        Tile = tile;
        tile.gameObject.transform.position = tile.Coor.ToWorld();
    }
}
