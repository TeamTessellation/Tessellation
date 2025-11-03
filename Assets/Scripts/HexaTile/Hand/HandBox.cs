using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandBox : MonoBehaviour, IPoolAble<TileSetData>
{
    public TileSet HoldTileSet { get; private set; }
    public bool IsUsed { get { return (HoldTileSet == null); } }

    private Action<HandBox> _downEvent;
    private Action<HandBox> _enterEvent;
    private Action<HandBox> _exitEvent;
    private EventTrigger _eventTrigger;

    public void Reset()
    {
        if (!IsUsed)
            Pool<TileSet>.Return(HoldTileSet);
    }

    private void Awake()
    {
        _eventTrigger = GetComponent<EventTrigger>();
    }

    public void Rotate()
    {

    }

    public void Use()
    {
        Pool<TileSet>.Return(HoldTileSet);
        HoldTileSet = null;
    }

    public void RegisterDownEvent(Action<HandBox> downEvent) => _downEvent += downEvent;
    public void RegisterEnterEvent(Action<HandBox> enterEvent) => _enterEvent = enterEvent;
    public void RegisterExitEvent(Action<HandBox> exitEvent) => _exitEvent = exitEvent;

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

        _eventTrigger.triggers = null;

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entry.callback.AddListener((data) => _downEvent(this));
        _eventTrigger.triggers.Add(entry);

        var entry2 = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry2.callback.AddListener((data) => _enterEvent(this));
        _eventTrigger.triggers.Add(entry2);

        var entry3 = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entry3.callback.AddListener((data) => _exitEvent(this));
        _eventTrigger.triggers.Add(entry3);
    }
}
