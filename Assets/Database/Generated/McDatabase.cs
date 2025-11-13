using System;
using System.Collections.Generic;
using UnityEngine;
using Database.Generated;

namespace Database
{
    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class McDatabase
    {
        public List<ItemData> ItemDataList = new List<ItemData>();
        public readonly List<string> ClassNames = new List<string> {
            "ItemData"
        };


        public T FindByName<T>(string name) where T : class
        {
            if (typeof(T) == null) return null;
            switch (typeof(T).Name)
            {
                default:
                    Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 타입: {typeof(T).Name}");
                    return null;
            }
            return null;
        }

        public T FindByIdentifier<T>(string identifier) where T : class
        {
            if (typeof(T) == null) return null;
            switch (typeof(T).Name)
            {
                default:
                    Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 타입: {typeof(T).Name}");
                    return null;
            }
            return null;
        }

        public void ClearAll()
        {
            ItemDataList.Clear();
        }



        private List<T> CreateInstance<T>(DataFrame df) where T : new()
        {
            object[] instances = ClassInstanceFactory.CreateInstance(df);
            List<T> list = new List<T>();
            foreach (var instance in instances)
            {
                if (instance is T typedInstance)
                {
                    list.Add(typedInstance);
                }
            }
            return list;
        }

        public void InitializeAll(List<DataFrame> dataFrames)
        {
            foreach (var df in dataFrames)
            {
                switch (df.name)
                {
                    case "ItemData":
                        ItemDataList = CreateInstance<ItemData>(df);
                        break;
                    default:
                        Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 이름: {df.name}");
                        break;
                }
            }
        }


        public void AddInstancesFromJsonList(string className, string json)
        {
            switch (className)
            {
                case "ItemData":
                    var newItemDataItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ItemData>>(json);
                    ItemDataList.AddRange(newItemDataItems);
                    break;
                default:
                    Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 이름: {className}");
                    break;
            }
        }


        public Type GetTypeByName(string className)
        {
            switch (className)
            {
                case "ItemData":
                    return typeof(ItemData);
                default:
                    Debug.LogWarning($"[MDatabase] 정의되지 않은 클래스 이름: {className}");
                    return null;
            }
        }
    }
}
