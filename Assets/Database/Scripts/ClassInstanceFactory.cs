using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Database.Generated;
using UnityEngine;

namespace Database
{
    public class ClassInstanceFactory
    {
        public static object[] CreateInstance(DataFrame df)
        {
            var type = ReflectionUtil.FindTypeByFullName("Database.Generated." + df.name);
            if (type == null)
            {
                Debug.LogError($"[ClassInstanceFactory] 타입을 찾을 수 없습니다: Database.Generated.{df.name}");
                return Array.Empty<object>();
            }

            int maxRow = df.data.Length;
            object[] instances = new object[maxRow];

            for (int i = 0; i < maxRow; i++)
            {
                var instance = Activator.CreateInstance(type);
                var row = df.data[i];

                for (int j = 0; j < row.Length; j++)
                {
                    var value = row[j];
                    var varName = df.GetVarName(j);

                    FieldInfo field = type.GetField(varName, BindingFlags.Public | BindingFlags.Instance);
                    if (field == null)
                    {
                        // 정의되지 않은 필드는 스킵
                        continue;
                    }

                    Type varType = field.FieldType;
                    object convertedValue = ConvertValue(varType, value);
                    field.SetValue(instance, convertedValue);
                }

                instances[i] = instance;
            }

            return instances;
        }



        public static object ConvertValue(Type targetType, string value)
        {
            // 널/공백 정리
            value = value?.Trim() ?? string.Empty;

            // 배열
            if (targetType.IsArray)
            {
                Type elementType = targetType.GetElementType()!;
                string[] elements = SplitCsv(value);
                Array arrayInstance = Array.CreateInstance(elementType, elements.Length);

                for (int i = 0; i < elements.Length; i++)
                {
                    object convertedValue = ConvertValue(elementType, elements[i]);
                    arrayInstance.SetValue(convertedValue, i);
                }
                return arrayInstance;
            }

            // 제네릭 List<T>
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                Type elementType = targetType.GetGenericArguments()[0];
                string[] elements = SplitCsv(value);
                var listInstance = Activator.CreateInstance(targetType);
                var addMethod = targetType.GetMethod("Add");
                foreach (var element in elements)
                {
                    object convertedValue = ConvertValue(elementType, element);
                    addMethod!.Invoke(listInstance, new[] { convertedValue });
                }
                return listInstance!;
            }

            // 열거형
            if (targetType.IsEnum)
            {
                if (string.IsNullOrEmpty(value))
                    return targetType.GetEnumValues().GetValue(0)!;
                if (int.TryParse(value, out int enumInt))
                    return Enum.ToObject(targetType, enumInt);
                return Enum.Parse(targetType, value, ignoreCase: true);
            }

            // 문자열
            if (targetType == typeof(string))
                return value;

            // 불리언(0/1도 허용)
            if (targetType == typeof(bool))
            {
                if (value == "0") return false;
                if (value == "1") return true;
                if (bool.TryParse(value, out bool b)) return b;
                return false;
            }

            // 숫자: 빈 문자열이면 0
            if (string.IsNullOrEmpty(value)) value = "0";
            else if (value == "null") value = "0";
            
            var ci = CultureInfo.InvariantCulture;

            // Debug.Log($"[ClassInstanceFactory] Converting '{value}' to {targetType.Name}");
            if (targetType == typeof(int))
                return int.Parse(value, ci);
            if (targetType == typeof(float))
                return float.Parse(value, ci);
            if (targetType == typeof(double))
                return double.Parse(value, ci);
            if (targetType == typeof(long))
                return long.Parse(value, ci);
            if (targetType == typeof(uint))
                return uint.Parse(value, ci);
            if (targetType == typeof(ulong))
                return ulong.Parse(value, ci);
            if (targetType == typeof(short))
                return short.Parse(value, ci);
            if (targetType == typeof(ushort))
                return ushort.Parse(value, ci);
            if (targetType == typeof(byte))
                return byte.Parse(value, ci);
            if (targetType == typeof(sbyte))
                return sbyte.Parse(value, ci);
            if (targetType == typeof(decimal))
                return decimal.Parse(value, ci);

            try
            {
                Type jsonUtilType = typeof(IJsonUtilitySupport);
                if(jsonUtilType.IsAssignableFrom(targetType))
                {
                    var instance = JsonUtility.FromJson(value, targetType);
                    return instance;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ClassInstanceFactory] IJsonUtilitySupport 변환 실패: {targetType.Name}, {e}");
            }

            try
            {
                Type loadableType = typeof(ILoadFromDatabaseString);
                if(loadableType.IsAssignableFrom(targetType))
                {
                    var instance = Activator.CreateInstance(targetType) as ILoadFromDatabaseString;
                    instance!.LoadFromDatabaseString(value);
                    return instance;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ClassInstanceFactory] ILoadFromDatabaseString 변환 실패: {targetType.Name}, {e}");
            }
            
            // 기타 타입은 ChangeType 시도
            Debug.LogWarning($"[ClassInstanceFactory] 알 수 없는 타입 변환 시도: {targetType.Name}, 값: '{value}'");
            return Convert.ChangeType(value, targetType, ci);
        }

        private static string[] SplitCsv(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return Array.Empty<string>(); 
            value = value.Replace("[", "").Replace("]", ""); // 대괄호 제거
            return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToArray();
        }
    }
}
