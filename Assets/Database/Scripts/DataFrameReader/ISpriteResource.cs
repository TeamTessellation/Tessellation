using UnityEngine;

namespace Cardevil.DataStructure
{
    public interface ISpriteResource
    {
        string IconUrl { get; }      // JSON에서 가져올 이미지 URL
        Sprite IconSprite { get; set; } // 최종적으로 로드될 Sprite
    }
}
