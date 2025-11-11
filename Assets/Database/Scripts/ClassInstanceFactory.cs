using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Database.Generated;
using Newtonsoft.Json;
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



        public static object ConvertValue(Type targetType, string value, int depth = 0)
        {
            // 널/공백 정리
            value = value?.Trim() ?? string.Empty;

            // 배열
            if (targetType.IsArray)
            {
                Type elementType = targetType.GetElementType()!;
                string[] elements = SplitToList(value);
                Array arrayInstance = Array.CreateInstance(elementType, elements.Length);

                for (int i = 0; i < elements.Length; i++)
                {
                    try
                    {

                        object convertedValue = ConvertValue(elementType, elements[i], depth + 1);
                        arrayInstance.SetValue(convertedValue, i);
                    }
                    catch (Exception e)
                    {
                        if(i == elements.Length - 1 && string.IsNullOrEmpty(elements[i]))
                        {
                            Array newArrayInstance = Array.CreateInstance(elementType, elements.Length - 1);
                            Array.Copy(arrayInstance, newArrayInstance, elements.Length - 1);
                            return newArrayInstance;
                        }
                        else
                        {
                            Debug.LogError($"[ClassInstanceFactory] 배열 요소 변환 실패: {targetType.Name}, 값: '{elements[i]}', 예외: {e}");
                            throw;
                        }
                    }
                }
                return arrayInstance;
            }

            // 제네릭 List<T>
            if (targetType.IsGenericType)
            {
                if (targetType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                {
                    Type elementType = targetType.GetGenericArguments()[0];
                    string[] elements = SplitToList(value);
                    var listInstance = Activator.CreateInstance(targetType);
                    var addMethod = targetType.GetMethod("Add");
                    foreach (var element in elements)
                    {
                        try
                        {
                            object convertedValue = ConvertValue(elementType, element, depth + 1);
                            addMethod!.Invoke(listInstance, new[] { convertedValue });
                        } 
                        catch (Exception e)
                        {
                            if(element == elements[elements.Length - 1] && string.IsNullOrEmpty(element))
                            {
                                // 마지막 요소가 빈 문자열인 경우 무시
                                continue;
                            }
                            else
                            {
                                Debug.LogError($"[ClassInstanceFactory] 리스트 요소 변환 실패: {targetType.Name}, 값: '{element}', 예외: {e}");
                                throw;
                            }
                        
                        }
                    }
                    return listInstance!;
                }
                if (targetType.GetGenericTypeDefinition() == typeof(Database.ListWrapper<>))
                {
                    Type elementType = targetType.GetGenericArguments()[0];
                    string[] elements = SplitToList(value);
                    var listWrapperInstance = Activator.CreateInstance(targetType);
                    var addMethod = targetType.GetMethod("Add");
                    foreach (var element in elements)
                    {
                        try
                        {
                            object convertedValue = ConvertValue(elementType, element, depth + 1);
                            addMethod!.Invoke(listWrapperInstance, new[] { convertedValue });
                        }
                        catch (Exception e)
                        {
                            if (element == elements[elements.Length - 1] && string.IsNullOrEmpty(element))
                            {
                                // 마지막 요소가 빈 문자열인 경우 무시
                                continue;
                            }
                            else
                            {
                                Debug.LogError($"[ClassInstanceFactory] 리스트래퍼 요소 변환 실패: {targetType.Name}, 값: '{element}', 예외: {e}");
                                throw;
                            }

                        }
                    }
                    return listWrapperInstance!;
                }
                
                
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
                    var instance = JsonConvert.DeserializeObject(value, targetType);
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
            
            if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(value, ci, DateTimeStyles.None, out DateTime dt))
                    return dt;
                return DateTime.MinValue;
            }
            if (targetType == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(value, ci, out TimeSpan ts))
                    return ts;
                return TimeSpan.Zero;
            }
            // 클래스 확인
            if (targetType.IsClass)
            {
                // Serializable 클래스는 JsonUtility로 파싱 시도
                if (Attribute.IsDefined(targetType, typeof(SerializableAttribute)))
                {
                    try
                    {
                        var instance = JsonUtility.FromJson(value, targetType);
                        return instance;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[ClassInstanceFactory] Serializable 클래스 변환 실패: {targetType.Name}, {e}");
                    }
                }

                if (Attribute.IsDefined(targetType, typeof(JsonConverterAttribute)))
                {
                    try
                    {
                        var instance = JsonConvert.DeserializeObject(value, targetType);
                        return instance!;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[ClassInstanceFactory] JsonConverter 클래스 변환 실패: {targetType.Name}, {e}");
                    }
                }
                
            }
            
            // 기타 타입은 ChangeType 시도
            Debug.LogWarning($"[ClassInstanceFactory] 알 수 없는 타입 변환 시도: {targetType.Name}, 값: '{value}'");
            return Convert.ChangeType(value, targetType, ci);
        }

        private static string[] SplitToList(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return Array.Empty<string>();
            if (value[0] == '[' && value[^1] == ']')
            {
                value = value.Substring(1, value.Length - 2);
            }
            if(value.Length == 0) return Array.Empty<string>();
            if(value.Length > 0 && value[value.Length - 1] == ',')
            {
                value = value.Substring(0, value.Length - 1);
            }

            var elements = new System.Collections.Generic.List<string>();
            int bracketCount = 0; // 중첩된 대괄호 [] 카운트
            int braceCount = 0;   // 중첩된 중괄호 {} 카운트
            int lastSplitIndex = 0;
            
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
        
                // 괄호 카운팅 업데이트
                if (c == '[') bracketCount++;
                else if (c == ']') bracketCount--;
                else if (c == '{') braceCount++;
                else if (c == '}') braceCount--;

                // 쉼표를 발견했을 때:
                // 1) 어떤 괄호(대괄호 또는 중괄호) 안에도 있지 않고 (최상위 레벨의 쉼표)
                // 2) 현재 문자가 쉼표일 경우
                if (c == ',' && bracketCount == 0 && braceCount == 0)
                {
                    elements.Add(value.Substring(lastSplitIndex, i - lastSplitIndex).Trim());
                    lastSplitIndex = i + 1;
                }
            }
            
            // 마지막 요소
            if (lastSplitIndex < value.Length)
            {
                elements.Add(value.Substring(lastSplitIndex).Trim());
            }
    
            return elements.ToArray();
        }
    }
}
