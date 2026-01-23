using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using SaveLoad;
using DG.Tweening;

public class HandBox : MonoBehaviour, IPoolAble<TileSetData>
{
    public TileSet HoldTileSet { get; private set; }
    public bool IsUsed { get { return (HoldTileSet == null); } }

    public List<Sprite> ItemSelectIcon;

    private Action<HandBox> _downEvent;
    private EventTrigger _eventTrigger;
    private SpriteRenderer _sprite;

    public void Reset()
    {
        if (!IsUsed)
        {
            Pool<TileSet>.Return(HoldTileSet);
            HoldTileSet = null;
        }
    }

    private void Awake()
    {
        _eventTrigger = GetComponent<EventTrigger>();
        _sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    public void Use()
    {
        Pool<TileSet>.Return(HoldTileSet);
        HoldTileSet = null;
    }

    public void RegisterDownEvent(Action<HandBox> downEvent) => _downEvent += downEvent;

    public void SetItemIcon(InputManager.eActiveItemType item)
    {
        _sprite.color = new Color(1, 1, 1, 1);
        if (ItemSelectIcon.Count > (int)item)
            _sprite.sprite = ItemSelectIcon[(int)item];
        else
            _sprite.sprite = ItemSelectIcon[ItemSelectIcon.Count - 1];

        for (int i = 0; i < HoldTileSet.Tiles.Count; i++)
        {
            HoldTileSet.Tiles[i].Sr.color = new Color(0.58f, 0.58f, 0.58f);
        }
        _sprite.transform.localPosition = Vector3.zero;
    }

    public void RemoveItemIcon()
    {
        _sprite.color = new Color(1, 1, 1, 0);
        if (IsUsed)
            return;
        for (int i = 0; i < HoldTileSet.Tiles.Count; i++)
        {
            HoldTileSet.Tiles[i].Sr.color = new Color(1f, 1f, 1f);
        }
    }

    public float StartScale = 0.3f;
    public float Duration = 1;
    public Ease ScaleEase;

    public void TileSetDrawEffect()
    {
        if (IsUsed)
            return;

        DOTween.Kill(this);

        float progress = StartScale;

        int maxRadius = 0;
        for (int j = 0; j < HoldTileSet.Data.Data.Count; j++)
        {
            maxRadius = Mathf.Max(HoldTileSet.Data.Data[j].Coor.CircleRadius, maxRadius);
        }
        float size = (maxRadius * 2 + 1 > 3) ? 5 / (Mathf.Sqrt(3) * (maxRadius * 2 + 1)) : 0.6f;

        DOTween.To(() => progress, x => { HoldTileSet.transform.localScale = Vector3.one * progress; progress = x; }, size, Duration)
            .SetEase(ScaleEase).OnComplete(() => HoldTileSet.transform.localScale = Vector3.one * size);
    }

    public void SetOnHand()
    {
        int maxRadius = 0;
        if(HoldTileSet == null)
            return;
        for (int j = 0; j < HoldTileSet.Data.Data.Count; j++)
        {
            maxRadius = Mathf.Max(HoldTileSet.Data.Data[j].Coor.CircleRadius, maxRadius);
        }
        float size = (maxRadius * 2 + 1 > 3) ? 5 / (Mathf.Sqrt(3) * (maxRadius * 2 + 1)) : 0.6f;
        HoldTileSet.transform.localScale = Vector2.one * size;

        Vector3 center = Vector3.zero;
        for (int i = 0; i < HoldTileSet.Tiles.Count; i++)
        {
            center += HoldTileSet.Tiles[i].transform.localPosition * HoldTileSet.transform.localScale.x;
        }
        center /= HoldTileSet.Tiles.Count;
        HoldTileSet.transform.localPosition = -center;
    }

    public async UniTask SetOnHandAsync()
    {
        await UniTask.NextFrame();
        SetOnHand();
    }

    public void Set(TileSetData data)
    {
        HoldTileSet = Pool<TileSet, TileSetData>.Get(data);
        HoldTileSet.transform.SetParent(transform, false);
        TileSetDrawEffect();

        SetOnHand();
        SetOnHandAsync().Forget();

        _eventTrigger.triggers = null;

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entry.callback.AddListener((data) => _downEvent(this));
        _eventTrigger.triggers.Add(entry);
    }

    public void SetOverwriteMode()
    {
        if (HoldTileSet == null) return;
        var tiles = HoldTileSet.Tiles;

        foreach (var tile in tiles)
            tile.SetOverwrite();
    }
}
