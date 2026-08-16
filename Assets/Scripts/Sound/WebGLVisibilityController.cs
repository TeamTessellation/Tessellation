using Sound;
using System.Runtime.InteropServices;
using UnityEngine;

public class WebGLVisibilityController : MonoBehaviour
{
    public AudioSource BGM1Audio;
    public AudioSource BGM2Audio;
    public AudioSource SFXAudio;

    // RegisterVisibilityChangeCallback lives in Assets/Plugins/WebGL/WebGLVisibility.jslib.
    // On WebGL, "__Internal" resolves against that jslib. On every other platform it means
    // "statically linked into the player", and IL2CPP fails at link time with an undefined
    // symbol. Mono hides this because it resolves DllImport lazily at runtime, so the guard
    // is what keeps IL2CPP Android builds linking.
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RegisterVisibilityChangeCallback(string gameObjectName);
#endif

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RegisterVisibilityChangeCallback(gameObject.name);
#endif
    }

    // Invoked from the jslib through SendMessage.
    public void OnPageVisibilityChanged(string state)
    {
        bool isHidden = state == "hidden" || state == "prerender";

        SoundManager.Instance.SetFocus(!isHidden);
    }
}
