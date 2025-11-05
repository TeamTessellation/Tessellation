using System.Collections.Generic;
using Collections;
using UnityEngine;

namespace SaveLoad
{
    /// <summary>
    /// 변수 컨테이너 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class VariableContainer
    {
        /// <summary>
        /// 다양한 타입의 값을 저장할 수 있는 변수 클래스입니다.
        /// 박싱을 피하기 위해 별도의 클래스로 구현되었습니다.
        /// </summary>
        [System.Serializable]
        public class Variable
        {
            [SerializeField] private string stringValue;
            [SerializeField] private int intValue;
            [SerializeField] private float floatValue;

            public string StringValue
            {
                get => stringValue;
                set => stringValue = value;
            }

            public int IntValue
            {
                get => intValue;
                set => intValue = value;
            }

            public float FloatValue
            {
                get => floatValue;
                set => floatValue = value;
            }
            
            public Variable Clone()
            {
                var cloned = MemberwiseClone() as Variable;
                return cloned;
            }
        }
        

        
        [SerializeField] private SerializableDictionary<string, Variable> items = new();

        public IReadOnlyDictionary<string, Variable> Items => items;
        
        

        public void SetString(string key, string value)
        {
            if(items.TryGetValue(key, out var variable))
            {
                variable.StringValue = value;
            }
            else
            {
                variable = new Variable {StringValue = value};
                items[key] = variable;
            }
        }
        public void SetInteger(string key, int value)
        {
            if(items.TryGetValue(key, out var variable))
            {
                variable.IntValue = value;
            }
            else
            {
                variable = new Variable {IntValue = value};
                items[key] = variable;
            }
        }
        public void SetFloat(string key, float value)
        {
            if(items.TryGetValue(key, out var variable))
            {
                variable.FloatValue = value;
            }
            else
            {
                variable = new Variable {FloatValue = value};
                items[key] = variable;
            }
        }
        
        public Variable GetVariable(string key)
        {
            if (items.TryGetValue(key, out var variable))
            {
                return variable;
            }
            return null;
        }

        public bool HasVariable(string key)
        {
            return items.ContainsKey(key);
        }
        public bool TryGetInteger(string key, out int value)
        {
            if (items.TryGetValue(key, out var variable))
            {
                value = variable.IntValue;
                return true;
            }
            value = default;
            return false;
        }
        public bool TryGetFloat(string key, out float value)
        {
            if (items.TryGetValue(key, out var variable))
            {
                value = variable.FloatValue;
                return true;
            }
            value = default;
            return false;
        }
        public bool TryGetString(string key, out string value)
        {
            if (items.TryGetValue(key, out var variable))
            {
                value = variable.StringValue;
                return true;
            }
            value = default;
            return false;
        }
        
        
        public Variable this[string key]
        {
            get => GetVariable(key);
        }
        
        public VariableContainer Clone()
        {
            var newContainer = new VariableContainer();
            foreach (var kvp in items)
            {
                newContainer.SetVariable(kvp.Key, kvp.Value.Clone());
            }
            return newContainer;
        }
        
        private void SetVariable(string key, Variable variable)
        {
            items[key] = variable;
        }
    }
}