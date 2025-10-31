using UnityEngine;
using UnityEngine.InputSystem;

public class FieldClickManager : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()).ToCoor());
        }
    }
}
