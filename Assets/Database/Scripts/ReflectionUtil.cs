using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Database
{
    public static class ReflectionUtil 
    {
        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();
        
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
        
        public static Type FindTypeByName(string typeName, bool searchAllAssembliesIfFailed = true)
        {
            Type t = null;
            Debug.LogWarning($"[ReflectionUtil] Type 이름 탐색 실패: {typeName}, 모든 어셈블리 탐색 시도");
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    t = asm.GetTypes().FirstOrDefault(type => type.Name == typeName);
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

        /// <summary>
        /// 제네릭 타입 T를 상속하는 모든 타입을 캐시를 사용하여 찾음
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> FindDerivedTypesWithCache<T>()
        {
            Type baseType = typeof(T);
            return FindDerivedTypesWithCache(baseType);
        }
        
        public static List<Type> FindDerivedTypesWithCache(Type baseType)
        {
            string cacheKey = baseType.FullName;
            if(cacheKey == null)
                throw new ArgumentException("Base type must have a full name.");
            
            if (TypeCache.ContainsKey(cacheKey))
            {
                return TypeCache.Values
                    .Where(t => t != baseType && baseType.IsAssignableFrom(t) && !t.IsAbstract)
                    .ToList();
            }

            var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t != baseType && baseType.IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            foreach (var type in derivedTypes)
            {
                TypeCache[type.FullName] = type;
            }

            return derivedTypes;
        }
        
        public static Attribute GetAttribute<T>(Type type) where T : Attribute
        {
            var attrs = type.GetCustomAttributes(typeof(T), inherit: false);
            if (attrs.Length > 0)
            {
                return attrs[0] as T;
            }
            return null;
        }
        
        public static bool IsDerivedFrom<T>(Type type) where T : class
        {
            Type baseType = typeof(T);
            return type != baseType && baseType.IsAssignableFrom(type);
        }
        
        public static bool IsDerivedFrom(Type type, Type baseType)
        {
            return type != baseType && baseType.IsAssignableFrom(type);
        }

        public static bool IsValidFullEnumType(string enumName)
        {
            return FindTypeByFullName(enumName) is Type t && t.IsEnum;
        }
        
        public static bool IsValidEnumType(string enumName, out string @namespace)
        {
            @namespace = null;
            Type t = FindTypeByName(enumName);
            if (t != null && t.IsEnum)
            {
                @namespace = t.Namespace;
                return true;
            }
            return false;
        }
    }
}