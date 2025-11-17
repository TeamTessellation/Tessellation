using UnityEngine;

public class LimitHandler : MonoBehaviour
{
    public static LimitHandler Instance {  get; private set; }

    public void Awake()
    {
        Instance = this;
    }
}
