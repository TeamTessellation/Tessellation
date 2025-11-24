using Sound;
using System.Runtime.InteropServices;
using UnityEngine;

public class WebGLVisibilityController : MonoBehaviour
{
    public AudioSource BGM1Audio;
    public AudioSource BGM2Audio;
    public AudioSource SFXAudio;

    [DllImport("__Internal")]
    private static extern void RegisterVisibilityChangeCallback(string gameObjectName);

    private void Start()
    {
        RegisterVisibilityChangeCallback(gameObject.name);
    }

    public void OnPageVisibilityChanged(string state)
    {
        bool isHidden = state == "hidden" || state == "prerender";

        SoundManager.Instance.SetFocus(!isHidden);
    }
}
