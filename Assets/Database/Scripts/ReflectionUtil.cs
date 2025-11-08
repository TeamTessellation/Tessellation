using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Database
{
    public static class ReflectionUtil 
    {
        public static Type FindTypeByFullName(string fullName, bool searchAllAssembliesIfFailed = true)
        {
            // Type.GetType 실패 시 모든 어셈블리 탐색
            var t = Type.GetType(fullName);
            if (t != null) return t;
            if (!searchAllAssembliesIfFailed) return null;

            Debug.LogWarning($"[ReflectionUtil] Type.GetType 실패: {fullName}, 모든 어셈블리 탐색 시도");
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    t = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
                    if (t != null) return t;
                }
                catch { /* 일부 동적/에디터 어셈블리 접근 실패 무시 */ }
            }
            return null;
        }
        
        public static List<Type> FindDerivedTypes<T>() where T : class
        {
            Type baseType = typeof(T);
            
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t != baseType && baseType.IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();
        }

        public static bool IsValidEnumType(string enumName)
        {
            return FindTypeByFullName(enumName) is Type t && t.IsEnum;
        }
    }
}