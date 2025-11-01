using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FieldClickManager : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Coordinate coor = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()).ToCoor();
#if UNITY_EDITOR
            //Debug.Log(coor);
#endif
            _tileClickEvent?.Invoke(coor);
        }
    }

    private Action<Coordinate> _tileClickEvent;

    public void RegisterClickEvent(Action<Coordinate> tileClickEvent) => _tileClickEvent += tileClickEvent;
    public void UnRegisterClickEvent(Action<Coordinate> tileClickEvent) => _tileClickEvent -= tileClickEvent;
}
