
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public static class ClassDefinitionFactory
    {
        // C# 기본 타입 화이트리스트(코드 생성 시 그대로 사용)
        // 소문자 비교 기준
        private static readonly HashSet<string> KnownTypes = new HashSet<string>
        {
            "string", "bool", "byte", "sbyte",
            "short", "ushort", "int", "uint",
            "long", "ulong", "float", "double", "decimal"
        };

        private static HashSet<string> KnownEnumTypes = new HashSet<string>() {

            };
        public static string GenerateClassDefinition(DataFrame df)
        {
            StringBuilder usingSb = new StringBuilder();
            usingSb.AppendLine("using System.Text;")
              .AppendLine("using System;")
              .AppendLine("using System.Collections.Generic;")
              .AppendLine();
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("namespace Database.Generated")
              .AppendLine("{")
              .AppendLine();
            sb.AppendLine("    [UnityEngine.Scripting.Preserve]")
              .AppendLine("    [Serializable]")
              .Append("    public partial class ")
              .Append(df.name)
              .AppendLine(": IDBData {")
              .AppendLine();

            for (int i = 0; i < df.MaxColumn; i++)
            {
                string type = df.types[i];
                if (string.IsNullOrEmpty(type)) continue;

                string tl = type.Trim().ToLower();
                if (tl == "null" || tl == "comment") continue;

                var full = GetVariableDefinition(df.varNames[i], type, df.comments[i], 2);
                sb.Append(full);
            }

            sb.AppendLine("    }")
              .AppendLine("}");
            
            return usingSb.ToString() + sb.ToString();
        }

        private static StringBuilder GetVariableDefinition(string name, string type, string comment = null, int indent = 0)
        {
            // 들여쓰기
            StringBuilder indentSb = new StringBuilder();
            for (int i = 0; i < indent; i++) indentSb.Append("    ");

            // 최종 출력 타입 결정
            string finalType = DecideType(type, out bool isKnown);

            // 알 수 없는 타입이면 참조 주석을 앞에 달아줌
            string finalComment = comment;
            if (!isKnown)
            {
                // 여기 오면 finalType == "string" 이고, type은 원본 표시용
                finalComment = $"(Reference:{type}) \n {indentSb}/// {comment ?? ""}";
            }

            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(finalComment))
            {
                sb.Append(indentSb)
                  .Append("/// <summary> ")
                  .Append(finalComment)
                  .AppendLine(" </summary>");
            }

            sb.Append(indentSb)
              .Append("public ")
              .Append(finalType)
              .Append(" ")
              .Append(name)
              .AppendLine(";");

            return sb;
        }

        // "int" / "string" 등 알려진 타입만 그대로 두고,
        // 그 외(배열, 제네릭, enum, 커스텀 등)는 모두 string으로 강등
        private static string DecideType(string original, out bool isKnown,int depth =0)
        {
            isKnown = false;
            if (string.IsNullOrEmpty(original)) return "string";
            string typeName = original.Trim();
            string typeNameLower = typeName.ToLower();

            if (IsKnown(typeNameLower)) { isKnown = true; return typeNameLower; }

            // Enum<T>
            if (typeNameLower.StartsWith("enum<") && typeNameLower.EndsWith(">"))
            {
                string enumName = typeName.Substring(5, typeName.Length - 6).Trim();
                
                // 전체 이름이 알려진 enum 타입이거나, 리플렉션으로 유효한 enum 타입이면 통과
                if (KnownEnumTypes.Contains(enumName) || ReflectionUtil.IsValidFullEnumType(enumName))
                {
                    isKnown = true;
                    return enumName;
                }
                // 부분 이름이 알려진 enum 타입이면 통과 + namespace 탐색
                if (ReflectionUtil.FindTypeByName(enumName) is { } enumType && enumType.IsEnum)
                {
                    if (enumType.FullName != null)
                    {
                        isKnown = true;
                        return enumType.FullName.Replace("+", "."); // 중첩 클래스인 경우 +가 들어올 수 있음
                    }
                }
                
                // 알 수 없는 enum 타입
                return "string";
            }
            
            if(typeNameLower.StartsWith("class<") && typeNameLower.EndsWith(">"))
            {
                string className = typeName.Substring(6, typeName.Length - 7).Trim();
                
                bool CheckClass(Type type)
                {
                    // SerializableAttribute 또는 JsonConverterAttribute가 붙어있는지 확인
                    if(ReflectionUtil.GetAttribute<SerializableAttribute>(type) is SerializableAttribute)
                    {
                        return true;
                    }
                    if (ReflectionUtil.GetAttribute<JsonConverterAttribute>(type) is JsonConverterAttribute)
                    {
                        return true;
                    }
                    
                    // // IDeserialize를 상속하는지 확인
                    // if (ReflectionUtil.IsDerivedFrom<IDeserialize>(type))
                    // {
                    //     return true;
                    // }
                    //
                    // // IDeserialize를 상속하지 않는 경우, 전체 IDeserializer 탐색
                    // var deserializerTypes = ReflectionUtil.FindDerivedTypesWithCache<IDeserializer>();
                    // foreach (var deserializerType in deserializerTypes)
                    // {
                    //     if(ReflectionUtil.GetAttribute<DeserializerAttribute>(deserializerType) is DeserializerAttribute attr)
                    //     {
                    //         if (attr.TargetType == type)
                    //         {
                    //             return true;
                    //         }
                    //     }
                    // }
                    return false;
                }

                if (ReflectionUtil.FindTypeByFullName(className) is { } type1)
                {
                    if (CheckClass(type1))
                    {
                        isKnown = true;
                        if (type1.FullName != null)
                        {
                            return type1.FullName.Replace("+", ".");
                        }
                    }
                }

                if (ReflectionUtil.FindTypeByName(className) is { } type2)
                {
                    if (CheckClass(type2))
                    {
                        isKnown = true;
                        if (type2.FullName != null)
                        {
                            return type2.FullName.Replace("+", ".");
                        }
                    }
                }
            }

            // List<T>
            if (typeNameLower.StartsWith("list<") && typeNameLower.EndsWith(">"))
            {
                // 괄호 안의 내부 타입 문자열을 추출
                string inner = typeName.Substring(5, typeName.Length - 6).Trim(); 
        
                // 내부 타입을 결정하기 위해 자기 자신(DecideType)을 재귀적으로 호출
                string determinedInnerType = DecideType(inner, out bool innerIsKnown,depth +1);

                if (innerIsKnown)
                {
                    if(depth ==0)
                    {
                        isKnown = true;
                        // 내부 타입이 결정되었으면 List<결정된타입> 형식으로 반환
                        return "List<" + determinedInnerType + ">";
                    }
                    else
                    {
                        isKnown = true;
                        return "List<" + determinedInnerType + ">";
                    }
                }
        
                // 내부 타입이 알려지지 않은 경우
                return "string";
            }

            return "string";
        }

        private static bool IsKnown(string tl)
        {
            return KnownTypes.Contains(tl);
        }
        
    }
}
