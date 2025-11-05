using UnityEngine;

namespace Utils
{
    public static class Extentions
    {
        
        
        public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
        {
            var component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }
    }
}