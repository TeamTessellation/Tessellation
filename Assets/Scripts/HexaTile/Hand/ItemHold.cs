using NUnit.Framework;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

[PoolSize(5)]
public class ItemHold : MonoBehaviour, IPoolAble
{
    public List<Sprite> ItemIcon;
    private EventTrigger _eventTrigger;
    private Image _image;

    private void Awake()
    {
        _eventTrigger = transform.GetComponent<EventTrigger>();
        _image = transform.GetChild(0).GetComponent<Image>();
    }
    public void SetItemIcon(InputManager.eActiveItemType item)
    {
        _image.color = new Color(1, 1, 1, 1);
        if (ItemIcon.Count > (int)item)
            _image.sprite = ItemIcon[(int)item];
        else
            _image.sprite = ItemIcon[ItemIcon.Count - 1];
    }

    public void RegisterClickEvent(Action<InputManager.eActiveItemType> action, InputManager.eActiveItemType item)
    {
        SetItemIcon(item);
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) => action?.Invoke(item));
        _eventTrigger.triggers = null;
        _eventTrigger.triggers.Add(entry);
    }

    public void Reset()
    {
        _eventTrigger.triggers = null;
    }
}
