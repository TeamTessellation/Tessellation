using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Cell
{
    public GameObject BG { get; private set; }
    public GameObject Lock { get; private set; }
    public Tile Tile { get; private set; }
    public bool IsEmpty => Tile == null;
    public bool IsLock = false;
    public Coordinate Coor { get; private set; }

    private Transform _cellRoot;

    [Header("Lock Effect")]
    Ease Ease = Ease.Linear;
    float Duration = 0.5f;

    // 최소 Cell 세팅
    public void Init(Coordinate coor, Transform bgRoot, Transform cellRoot)
    {
        Coor = coor;
        BG = Pool.Get("BGTile");
        BG.transform.SetParent(bgRoot);
        BG.transform.localPosition = coor.ToWorld();
        Tile = null;
        IsLock = false;
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

    public void LockCell(Transform lockRoot)
    {
        UnSet();
        Lock = Pool.Get("LockTile");
        Lock.transform.SetParent(lockRoot);
        Lock.transform.localPosition = Coor.ToWorld();
        IsLock = true;
        LockEffect().Forget();
    }

    private async UniTask LockEffect()
    {
        Lock.transform.localScale = new(0, 0, 1);

        float progress = 0;
        await DOTween.To(() => progress, x => { Lock.transform.localScale = new(progress, progress, 1); progress = x; }, 1, Duration)
            .SetEase(Ease)
            .ToUniTask();

        Lock.transform.localScale = Vector3.one;
    }

    public void UnLock()
    {
        if (!IsLock)
            return;
        IsLock = false;
        Pool.Return(Lock);
    }
    
    public void SetSize(float size)
    {
        BG.transform.localScale = new Vector3(size, size, 1);
        if (Tile != null)
        {
            Tile.transform.localScale = new Vector3(size, size, 1);
        }
    }
    public void ResetSize()
    {
        BG.transform.localScale = Vector3.one;
        if (Tile != null)
        {
            Tile.transform.localScale = Vector3.one;
        }
    }
    

    /// <summary>
    /// 해당 cell에 전달받은 Tile 배치
    /// </summary>
    /// <param name="tile">배치할 Tile</param>
    public void Set(Tile tile)
    {
        UnSet();

        Tile = tile;
        tile.Coor = Coor;
        tile.transform.SetParent(_cellRoot, true);
        tile.gameObject.transform.position = tile.Coor.ToWorld(Field.Instance.TileOffset);
    }
}
