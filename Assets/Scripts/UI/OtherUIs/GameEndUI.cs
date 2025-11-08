using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI.OtherUIs
{
    public class GameEndUI : MonoBehaviour
    {
        [Header("")]
        
        [Header("숫자들 관련 설정")]
        [SerializeField,Tooltip("터치시 스킵가능")] private bool touchToSkip;
        [SerializeField,Tooltip("숫자 올라가는거 시간")] private float counterDuration = 0.25f;
        [SerializeField,Tooltip("각 숫자가 올라가는 시간 딜레이")] private float counterDelay = 0.1f;



        public async UniTask ShowGameEndUIAsync(CancellationToken cancellationToken)
        {
            
             
        }
    }
}