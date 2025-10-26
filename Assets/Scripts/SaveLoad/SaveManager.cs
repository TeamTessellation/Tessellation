using System.Collections.Generic;
using Core;
using Machamy.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaveLoad
{
    
    /// <summary>
    /// 저장 데이터를 나타내는 구조체입니다.
    /// 변수 컨테이너가 존재하지만, 내장 변수를 우선적으로 사용해야 합니다.
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        public int TurnCount;
        public int Score;
        
        /// <summary>
        /// 변수 컨테이너. 우선적으로 내장 변수를 사용하는 것이 권장됩니다.
        /// </summary>
        [field:SerializeField] public VariableContainer Variables { private set; get; }
        
        public SaveData()
        {
            Variables = new VariableContainer();
        }
        
        
        public bool ContainsKey(string key)
        {
            // 일반 변수들은 VariableContainer에서 처리
            return Variables.Items.ContainsKey(key);
        }
        
        public void SaveVariable(string key, int value)
        {
            Variables.SetInteger(key, value);
        }
        public void SaveVariable(string key, float value)
        {
            Variables.SetFloat(key, value);
        }
        public void SaveVariable(string key, string value)
        {
            Variables.SetString(key, value);
        }
        public VariableContainer.Variable GetVariable(string key)
        {
            return this[key];
        }
        public int GetIntVariable(string key, int defaultValue = 0)
        {
            var variable = this[key];
            if (variable != null)
            {
                return variable.IntValue;
            }
            return defaultValue;
        }

        public float GetFloatVariable(string key, float defaultValue = 0f)
        {
            var variable = this[key];
            if (variable != null)
            {
                return variable.FloatValue;
            }
            return defaultValue;
        }
        public string GetStringVariable(string key, string defaultValue = "")
        {
            var variable = this[key];
            if (variable != null)
            {
                return variable.StringValue;
            }
            return defaultValue;
        }
        
        public VariableContainer.Variable this[string key]
        {
            get
            {
                // 내장 변수들은 여기서 처리
                if (key == "TurnCount")
                {
                    return new VariableContainer.Variable { IntValue = TurnCount };
                }
                if (key == "Score")
                {
                    return new VariableContainer.Variable { IntValue = Score };
                }
                
                
                // 일반 변수들은 VariableContainer에서 처리
                if (Variables.Items.TryGetValue(key, out var variable))
                {
                    return variable;
                }

                return null;
            }
        }
        
        public SaveData Clone()
        {
            SaveData cloned = MemberwiseClone() as SaveData;
            cloned.Variables = this.Variables.Clone();
            return cloned;
        }
    }

    /// <summary>
    /// 데이터를 저장하고 불러오는 기능을 제공하는 인터페이스입니다.
    /// </summary>
    public interface ISavable
    {
        void LoadData(SaveData data);
        void SaveData(ref SaveData data);
    }


    /// <summary>
    /// 데이터를 저장하고 불러오는 기능을 관리하는 클래스입니다.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SaveManager : Singleton<SaveManager>
    {
        public override bool IsDontDestroyOnLoad => false;

        private List<ISavable> _savables = new();
        
        private static readonly List<ISavable> _pendingSavables = new();
        public static void RegisterPendingSavable(ISavable savable)
        {
            if (_instance != null)
            {
                Instance.RegisterSavable(savable);
            }
            else
            {
                // 인스턴스가 아직 생성되지 않은 경우, 나중에 등록하도록 대기합니다.
                _pendingSavables.Add(savable);
            }
        }

        protected override void AfterAwake()
        {
            base.AfterAwake();
            
            // 대기 중인 ISavable들을 등록합니다.
            foreach (var savable in _pendingSavables)
            {
                RegisterSavable(savable);
            }
        }

        public void RegisterSavable(ISavable savable)
        {
            if (!_savables.Contains(savable))
            {
                _savables.Add(savable);
            }
        }
        public void UnregisterSavable(ISavable savable)
        {
            if (_savables.Contains(savable))
            {
                _savables.Remove(savable);
            }
        }
        public void UnregisterAllSavables()
        {
            _savables.Clear();
        }
        
        
        public SaveData CreateCurrentSaveData()
        {
            SaveData data = new SaveData();
            LogEx.Log($"Creating Save Data from {_savables.Count} savables.");
            foreach (var savable in _savables)
            {
                savable.SaveData(ref data);
            }
            return data;
        }
        
        public void LoadSaveData(SaveData data)
        {
            LogEx.Log($"Loading Save Data to {_savables.Count} savables.");
            foreach (var savable in _savables)
            {
                savable.LoadData(data);
            }
        }
    }

}