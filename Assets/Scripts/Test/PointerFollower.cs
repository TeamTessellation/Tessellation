using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PointerFollower : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (Camera.main == null) return;
        Mouse mouse = Mouse.current;
        if (mouse == null) return;
        Vector2 mousePosition = mouse.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane));
        transform.position = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
    }
}
