using System;
using System.Collections.Generic;
using Core;
using Machamy.Utils;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaveLoad
{
    
    /// <summary>
    /// 저장 데이터를 나타내는 구조체입니다.
    /// 변수 컨테이너가 존재하지만, 내장 변수를 우선적으로 사용해야 합니다.
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        public int TurnCount;
        public int Score;
        public int HandCount;
        public int FieldSize;
        public List<OffsetTileData> FieldTileData;
        public TileSetData[] HandData;
        public PlayerStatus PlayerStatus;
        
        /// <summary>
        /// 변수 컨테이너. 우선적으로 내장 변수를 사용하는 것이 권장됩니다.
        /// </summary>
        [field:SerializeField] public VariableContainer Variables { private set; get; }
        
        public GameData()
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
        
        public GameData Clone()
        {
            GameData cloned = MemberwiseClone() as GameData;
            cloned.Variables = this.Variables.Clone();
            return cloned;
        }
    }

    /// <summary>
    /// 데이터를 저장하고 불러오는 기능을 제공하는 인터페이스입니다.
    /// </summary>
    public interface ISaveTarget
    {
        Guid Guid { get; init; }
        void LoadData(GameData data);
        void SaveData(ref GameData data);
    }
    
    /// <summary>
    /// 데이터가 저장될 수 있는 단위입니다.
    /// Guid를 통해 고유하게 식별됩니다.
    /// </summary>
    public interface ISaveData
    {
        Guid Guid { get; init; }
    }


    /// <summary>
    /// 데이터를 저장하고 불러오는 기능을 관리하는 클래스입니다.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        public override bool IsDontDestroyOnLoad => false;

        private List<ISaveTarget> _savables = new();

        private static readonly List<ISaveTarget> _pendingSavables = new();

        public static void RegisterPendingSavable(ISaveTarget saveTarget)
        {
            if (_instance != null)
            {
                Instance.RegisterSaveTarget(saveTarget);
            }
            else
            {
                // 인스턴스가 아직 생성되지 않은 경우, 나중에 등록하도록 대기합니다.
                _pendingSavables.Add(saveTarget);
            }
        }

        protected override void AfterAwake()
        {
            base.AfterAwake();

            // 대기 중인 ISavable들을 등록합니다.
            foreach (var savable in _pendingSavables)
            {
                RegisterSaveTarget(savable);
            }
        }

        public void RegisterSaveTarget(ISaveTarget saveTarget)
        {
            if (!_savables.Contains(saveTarget))
            {
                _savables.Add(saveTarget);
            }
        }

        public void UnregisterSaveTarget(ISaveTarget saveTarget)
        {
            if (_savables.Contains(saveTarget))
            {
                _savables.Remove(saveTarget);
            }
        }

        public void UnregisterAllSaveTarget()
        {
            _savables.Clear();
        }


        public GameData CreateCurrentSaveData()
        {
            GameData data = new GameData();
            LogEx.Log($"Creating Save Data from {_savables.Count} savables.");
            foreach (var savable in _savables)
            {
                savable.SaveData(ref data);
            }

            return data;
        }

        public void LoadSaveData(GameData data)
        {
            LogEx.Log($"Loading Save Data to {_savables.Count} savables.");
            foreach (var savable in _savables)
            {
                savable.LoadData(data);
            }
        }


        /// <summary>
        /// 간단한 저장 기능을 제공합니다.
        /// </summary>
        public void SimpleSave()
        {
            PlayerPrefs.SetString("SimpleSaveData", JsonUtility.ToJson(CreateCurrentSaveData()));
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 간단한 불러오기 기능을 제공합니다.
        /// </summary>
        /// <returns>></returns>
        public bool SimpleLoad(string savePath = "Default")
        {
            string json = PlayerPrefs.GetString($"save_{savePath}", "");
            if (string.IsNullOrEmpty(json))
                return false;
            try
            {
                GameData data = JsonUtility.FromJson<GameData>(json);
                LoadSaveData(data);
                return true;
            }
            catch (Exception e)
            {
                LogEx.LogError("Failed to load simple save data: " + e.Message);
                return false;
            }
        }
        
        /// <summary>
        /// 세이브데이터 여부를 확실하게 판단합니다.
        /// </summary>
        /// <returns></returns>
        public bool HasSaveSimpleReliable()
        {
            return SimpleLoad();
        }
        
        /// <summary>
        /// 저장 데이터가 존재하는지 간단하게 확인합니다.
        /// </summary>
        /// <returns></returns>
        public bool HasSimpleSave()
        {
            return PlayerPrefs.HasKey("SimpleSaveData");
        }
}

}