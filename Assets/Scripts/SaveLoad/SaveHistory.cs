using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveLoad
{
    /// <summary>
    /// SaveData를 선형적으로 저장하는 Container 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class SaveHistory : IReadOnlyList<GameData>
    {
        [SerializeField] private List<GameData> _saves = new();

        public IReadOnlyList<GameData> Saves => _saves;


        #region IReadOnlyList

        public int Count => _saves.Count;
        public bool IsReadOnly => false;
        public GameData this[int index] => _saves[index];


        public IEnumerator<GameData> GetEnumerator()
        {
            return _saves.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        #endregion

        /// <summary>
        /// 모든 저장 데이터를 삭제합니다.
        /// </summary>
        public void Clear()
        {
            _saves.Clear();
        }

        /// <summary>
        /// 새로운 저장 데이터를 추가합니다.
        /// 복제된 데이터가 저장됩니다.
        /// </summary>
        /// <param name="data"></param>
        public void Add(GameData data)
        {
            _saves.Add(data.Clone());
        }

        
        /// <summary>
        /// 마지막 저장 데이터를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public GameData GetLastSave()
        {
            if (_saves.Count == 0)
                return null;

            return _saves[^1];
        }
        
        /// <summary>
        /// 지정한 인덱스의 저장 데이터를 반환합니다.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GameData GetSaveAt(int index)
        {
            return _saves[index];
        }
        
        /// <summary>
        /// 마지막 저장 데이터를 제거하고 반환합니다.
        /// </summary>
        /// <returns></returns>
        public GameData PopLastSave()
        {
            if (_saves.Count == 0)
                return default;

            var lastIndex = _saves.Count - 1;
            var lastSave = _saves[lastIndex];
            _saves.RemoveAt(lastIndex);
            return lastSave;
        }
        
    }
}