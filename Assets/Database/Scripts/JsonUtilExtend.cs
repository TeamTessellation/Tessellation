using System;
using System.Collections.Generic;
using UnityEngine;

namespace Database
{
    public static class JsonUtilExtend
    {
        [Serializable]
        public class JsonList
        {
            public List<string> list;
        }
        
        public static List<string> GetJsonList(string json)
        {
            JsonList jsonList = JsonUtility.FromJson<JsonList>(json);
            return jsonList.list;
        }
        
        public static List<T> FromJsonList<T>(string json)
        {
            JsonList jsonList = JsonUtility.FromJson<JsonList>(json);
            List<T> list = new List<T>();
            foreach (var item in jsonList.list)
            {
                T obj = JsonUtility.FromJson<T>(item);
                if (obj != null)
                    list.Add(obj);
                else
                {
                    Debug.LogError($"Failed to deserialize item: {item}");
                }
            }

            return list;
        }
        
        public static string ToJsonList<T>(List<T> list)
        {
            JsonList jsonList = new JsonList();
            jsonList.list = new List<string>();
            foreach (var item in list)
            {
                jsonList.list.Add(JsonUtility.ToJson(item));
            }

            return JsonUtility.ToJson(jsonList);
        }
        
        public static string ToJsonList<T>(T[] array)
        {
            JsonList jsonList = new JsonList();
            jsonList.list = new List<string>();
            foreach (var item in array)
            {
                jsonList.list.Add(JsonUtility.ToJson(item));
            }

            return JsonUtility.ToJson(jsonList);
        }
    }
}