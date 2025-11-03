using NUnit.Framework;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[PoolSize(5)]
public class ItemHold : MonoBehaviour, IPoolAble
{
    private EventTrigger _eventTrigger;
    private Action _itemUseAction;
    private bool _dragItemHold;
    private Camera _cam;
    private GameObject _icon;

    private void Awake()
    {
        _cam = Camera.main;
        _icon = transform.GetChild(0).gameObject;
        _eventTrigger = transform.GetComponent<EventTrigger>();
    }

    public void RegisterClickEvent(Action<InputManager.Item> action, Action itemUseAction, InputManager.Item item)
    {
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entry.callback.AddListener((data) => action?.Invoke(item));
        entry.callback.AddListener((data) => ItemHoldMouseDown());
        _eventTrigger.triggers = null;
        _eventTrigger.triggers.Add(entry);
        _itemUseAction += itemUseAction;
    }

    private void ItemHoldMouseDown()
    {
        _dragItemHold = true;
    }

    private void Update()
    {
        if (_dragItemHold)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 worldPos = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _cam.nearClipPlane));

            _icon.transform.position = worldPos;

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _dragItemHold = false;
                _icon.transform.localPosition = Vector3.zero;
                _itemUseAction?.Invoke();
            }
        }
    }

    public void Reset()
    {
        _eventTrigger.triggers = null;
    }
}
