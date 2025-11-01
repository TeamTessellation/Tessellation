using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandBox : MonoBehaviour, IPoolAble<TileSetData>
{
    public TileSet HoldTileSet { get; private set; }
    public bool IsUsed { get { return (HoldTileSet == null); } }

    private Action<HandBox> _clickEvent;
    private EventTrigger _eventTrigger;

    public void Reset()
    {
        Pool<TileSet>.Return(HoldTileSet);
    }

    private void Awake()
    {
        _eventTrigger = GetComponent<EventTrigger>();
    }

    public void Use() => HoldTileSet = null;

    public void RegisterClickEvent(Action<HandBox> clickEvent) => _clickEvent += clickEvent;

    public void Set(TileSetData data)
    {
        HoldTileSet = Pool<TileSet, TileSetData>.Get(data);
        HoldTileSet.transform.SetParent(transform, false);
        HoldTileSet.transform.localPosition = Vector3.zero;

        int maxRadius = 0;
        for (int j = 0; j < data.Data.Count; j++)
        {
            maxRadius = Mathf.Max(data.Data[j].Coor.CircleRadius, maxRadius);
        }
        float size = (maxRadius * 2 + 1 > 3) ? 5 / (Mathf.Sqrt(3) * (maxRadius * 2 + 1)) : 1;
        HoldTileSet.transform.localScale = Vector2.one * size;

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entry.callback.AddListener((data) => _clickEvent(this));
        _eventTrigger.triggers.Clear();
        _eventTrigger.triggers.Add(entry);
    }
}
