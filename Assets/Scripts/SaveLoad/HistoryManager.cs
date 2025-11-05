using Core;
using UnityEngine;

namespace SaveLoad
{
    /// <summary>
    /// 선형적으로 저장 기록을 관리하는 클래스입니다.
    /// </summary>
    public class HistoryManager : Singleton<HistoryManager>
    {
        public override bool IsDontDestroyOnLoad => false;

        [SerializeField] private SaveHistory _saveHistory = new SaveHistory();

        public SaveHistory SaveHistory => _saveHistory;
        
        /// <summary>
        /// 모든 저장 기록을 삭제합니다.
        /// </summary>
        public void ClearHistory()
        {
            _saveHistory.Clear();
        }
        

        /// <summary>
        /// 마지막 저장 기록을 불러옵니다.
        /// </summary>
        public void LoadLastSave()
        {
            var lastSave = _saveHistory.GetLastSave();
            SaveManager.Instance.LoadSaveData(lastSave);
        }
        
        /// <summary>
        /// 마지막 저장 기록을 불러오고 기록에서 제거합니다.
        /// </summary>
        /// <returns></returns>
        public GameData LoadAndPopLastSave()
        {
            var lastSave = _saveHistory.PopLastSave();
            SaveManager.Instance.LoadSaveData(lastSave);
            return lastSave;
        }
        
        /// <summary>
        /// 현재 상태를 저장 기록에 추가합니다.
        /// </summary>
        public void SaveCurrentState()
        {
            var currentSave = SaveManager.Instance.CreateCurrentSaveData();
            AddSave(currentSave);
        }
        
        
        private void AddSave(GameData data)
        {
            _saveHistory.Add(data);
        }
    }
    
    #if UNITY_EDITOR
    
    [UnityEditor.CustomEditor(typeof(HistoryManager))]
    public class HistoryManagerEditor : UnityEditor.Editor
    {
        [UnityEditor.MenuItem("SaveLoad/Clear History")]
        public static void ClearHistory()
        {
            HistoryManager.Instance.ClearHistory();
            UnityEditor.EditorUtility.DisplayDialog("History Cleared", "All save history has been cleared.", "OK");
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            HistoryManager instance = (HistoryManager)target;
            
            // 현재 상태 저장 / 직전 상태 불러오기
            using (new UnityEditor.EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save Current State"))
                {
                    instance.SaveCurrentState();
                }

                if (GUILayout.Button("Load Last Save"))
                {
                    instance.LoadLastSave();
                }

                if (GUILayout.Button("Load and Pop Last Save"))
                {
                    instance.LoadAndPopLastSave();
                }
            }
        }
    }
    
    #endif
}