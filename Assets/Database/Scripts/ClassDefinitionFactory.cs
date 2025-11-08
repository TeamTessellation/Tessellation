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
            "Cardevil.Utils.Directions.Direction",
            "Define.RareType",
            "Define.SlotRewardType",
            "Cardevil.Cards.Evaluations.HandRanking",
            "Cardevil.Relics.EffectExcute",
            "Cardevil.Relics.EffectEvaluation"
            };
        public static string GenerateClassDefinition(DataFrame df)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System.Text;")
              .AppendLine("using System;")
              .AppendLine("using System.Collections.Generic;")
              .AppendLine();

            sb.AppendLine("namespace Database.Generated")
              .AppendLine("{")
              .AppendLine();
            sb.AppendLine("    [UnityEngine.Scripting.Preserve]")
              .AppendLine("    [Serializable]")
              .Append("    public class ")
              .Append(df.name)
              .AppendLine("    {")
              .AppendLine();

            for (int i = 0; i < df.MaxColumn; i++)
            {
                string type = df.types[i];
                if (string.IsNullOrEmpty(type)) continue;

                string tl = type.Trim().ToLower();
                if (tl == "null" || tl == "comment") continue;

                sb.Append(GetVariableDefinition(df.varNames[i], type, df.comments[i], 2));
            }

            sb.AppendLine("    }")
              .AppendLine("}");

            return sb.ToString();
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
        private static string DecideType(string original, out bool isKnown)
        {
            isKnown = false;
            if (string.IsNullOrEmpty(original)) return "string";
            string t = original.Trim();
            string tl = t.ToLower();

            if (IsKnown(tl)) { isKnown = true; return tl; }

            // Enum<T>
            if (tl.StartsWith("enum<") && tl.EndsWith(">"))
            {
                string enumName = t.Substring(5, t.Length - 6).Trim();
                if (KnownEnumTypes.Contains(enumName) || ReflectionUtil.IsValidEnumType(enumName))
                {
                    isKnown = true;
                    return enumName;
                }
                return "string";
            }
            
            if(tl.StartsWith("class<") && tl.EndsWith(">"))
            {
                string className = t.Substring(6, t.Length - 7).Trim();
                if (ReflectionUtil.FindTypeByFullName(className) is { } type)
                {
                    isKnown = true;
                    return className;
                }
            }

            // List<T>
            if (tl.StartsWith("list<") && tl.EndsWith(">"))
            {
                string inner = t.Substring(5, t.Length - 6).Trim(); 
                string innerLower = inner.ToLower();

                if (IsKnown(innerLower))
                {
                    isKnown = true;
                    return "List<" + innerLower + ">";
                }
                if (innerLower.StartsWith("enum<") && innerLower.EndsWith(">"))
                {
                    string enumName = inner.Substring(5, inner.Length - 6).Trim();
                    if (KnownEnumTypes.Contains(enumName) || ReflectionUtil.IsValidEnumType(enumName))
                    {
                        isKnown = true;
                        return "List<" + enumName + ">";
                    }
                }
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
