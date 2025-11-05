using UnityEngine;
using UnityEngine.UI;

public class StageClearEffect : MonoBehaviour
{
    private Image _image;
    float _tileSize = 1;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.material = new Material(_image.material); // 인스턴스 복사
        InitEffect();
    }

    private void InitEffect()
    {
        _tileSize = _image.material.GetFloat("_TileSize");
        float dist = 0 - Camera.main.transform.position.z;
        int start = ((Vector2)(Camera.main.ScreenToWorldPoint(new Vector3(0, 0, dist)))).ToCoor(_tileSize).Pos.x;
        int end = ((Vector2)(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, dist)))).ToCoor(_tileSize).Pos.x;

        _image.material.SetFloat("_XCount", end - start + 1);
        _image.material.SetFloat("_StartX", start);
        _image.material.SetFloat("_XEnd", end);
    }

    public void SetProgress(float progress)
    {
        _image.material.SetFloat("_Progress", progress);
    }
}
