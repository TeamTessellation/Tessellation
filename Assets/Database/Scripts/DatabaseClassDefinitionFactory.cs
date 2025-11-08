using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Database
{
    public class DatabaseClassDefinitionFactory
    {
        public static string GenerateDatabaseClass(string dbClassName, List<string> classNames, List<string> findTargetVariables)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using Database.Generated;");
            sb.AppendLine();
            sb.AppendLine("namespace Database");
            sb.AppendLine("{");
            sb.AppendLine("    [UnityEngine.Scripting.Preserve]");
            sb.AppendLine("    [Serializable]");
            sb.AppendLine($"    public class {dbClassName}");
            sb.AppendLine("    {");
            foreach (var name in classNames)
            {
                sb.AppendLine($"        public List<{name}> {name}List = new List<{name}>();");
            }
            sb.AppendLine("        public readonly List<string> ClassNames = new List<string> {");
            for (int i = 0; i < classNames.Count-1; i++)
            {
                var name = classNames[i];
                sb.Append($"            \"{name}\"");
                sb.AppendLine(",");
            }
            if (classNames.Count > 0)
            {
                var name = classNames[classNames.Count - 1];
                sb.Append($"            \"{name}\"");
                sb.AppendLine();
            }
            sb.AppendLine("        };");
            

            sb.AppendLine();
            sb.AppendLine();
            
            sb.Append(GetFindTargetMethod(classNames, findTargetVariables));
            
            sb.AppendLine("        public void ClearAll()");
            sb.AppendLine("        {");
            foreach (var name in classNames)
            {
                sb.AppendLine($"            {name}List.Clear();");
            }
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine();
            sb.Append(@"
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
        }");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("        public void InitializeAll(List<DataFrame> dataFrames)");
            sb.AppendLine("        {");
            sb.AppendLine("            foreach (var df in dataFrames)");
            sb.AppendLine("            {");
            sb.AppendLine("                switch (df.name)");
            sb.AppendLine("                {");
            foreach (var name in classNames)
            {
                sb.AppendLine($"                    case \"{name}\":");
                sb.AppendLine($"                        {name}List = CreateInstance<{name}>(df);");
                sb.AppendLine("                        break;");
            }
            sb.AppendLine("                    default:");
            sb.AppendLine("                        Debug.LogWarning($\"[MDatabase] 정의되지 않은 클래스 이름: {df.name}\");");
            sb.AppendLine("                        break;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine();
            sb.Append(GetAddInstancesFromJsonListMethod(classNames));
            sb.AppendLine();
            sb.AppendLine();
            sb.Append(GetGetTypeByNameMethod(classNames));
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
        
        private static StringBuilder GetFindTargetMethod(List<string> classNames, List<string> findTargetVariables)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            bool IsValidVarName(string varName)
            { 
                if(string.IsNullOrEmpty(varName)) return false;
                if(!char.IsLetter(varName[0])) return false;
                string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
                foreach (var c in varName)
                {
                    if (!validChars.Contains(c)) return false;
                }
                return true;
            }
            
            foreach (var varName in findTargetVariables)
            {
                string pascalVarName = char.ToUpper(varName[0]) + varName.Substring(1);
                string trimmedVarName = varName.Trim();
                if (!IsValidVarName(trimmedVarName))
                {
                    Debug.LogWarning($"[MDatabase] FindTargetVariables에 유효하지 않은 변수명이 포함되어 있습니다: '{varName}'");
                    continue;
                }
                
                sb.AppendLine($"        public T FindBy{pascalVarName}<T>(string {varName}) where T : class");
                sb.AppendLine("        {");
                sb.AppendLine("            if (typeof(T) == null) return null;");
                sb.AppendLine("            switch (typeof(T).Name)");
                sb.AppendLine("            {");
                foreach (var className in classNames)
                {
                    Type type = ReflectionUtil.FindTypeByFullName($"Database.Generated.{className}");
                    // varName이 멤버로 있는경우
                    if (type == null) continue;
                    var field = type.GetField(varName);
                    if (field == null) continue;
                    sb.AppendLine($"                case \"{className}\":");
                    sb.AppendLine($"                    foreach (var instance in {className}List)");
                    sb.AppendLine($"                    {{");
                    sb.AppendLine($"                        if (instance.{varName} == {varName.ToLower()})");
                    sb.AppendLine($"                            return instance as T;");
                    sb.AppendLine($"                    }}");
                    sb.AppendLine("                    break;");
                }
                sb.AppendLine("                default:");
                sb.AppendLine("                    Debug.LogWarning($\"[MDatabase] 정의되지 않은 클래스 타입: {typeof(T).Name}\");");
                sb.AppendLine("                    return null;");
                sb.AppendLine("            }");
                sb.AppendLine("            return null;");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            return sb;
        }
        
        private static StringBuilder GetAddInstancesFromJsonListMethod(List<string> classNames)
        {
            StringBuilder sb = new StringBuilder();
            // 메소드 파라미터 이름을 jsonList -> json으로 변경하여 명확하게 함
            sb.AppendLine("        public void AddInstancesFromJsonList(string className, string json)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (className)");
            sb.AppendLine("            {");
            foreach (var name in classNames)
            {
                sb.AppendLine($"                case \"{name}\":");

                // --- 수정된 부분 ---
                // 기존의 비표준 JsonUtilExtend 대신 표준 Newtonsoft.Json을 사용하도록 변경
                sb.AppendLine($"                    var new{name}Items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<{name}>>(json);");
                // ---

                sb.AppendLine($"                    {name}List.AddRange(new{name}Items);");
                sb.AppendLine("                    break;");
            }
            sb.AppendLine("                default:");
            sb.AppendLine("                    Debug.LogWarning($\"[MDatabase] 정의되지 않은 클래스 이름: {className}\");");
            sb.AppendLine("                    break;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            return sb;
        }

        private static StringBuilder GetGetTypeByNameMethod(List<string> classNames)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("        public Type GetTypeByName(string className)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (className)");
            sb.AppendLine("            {");
            foreach (var name in classNames)
            {
                sb.AppendLine($"                case \"{name}\":");
                sb.AppendLine($"                    return typeof({name});");
            }
            sb.AppendLine("                default:");
            sb.AppendLine("                    Debug.LogWarning($\"[MDatabase] 정의되지 않은 클래스 이름: {className}\");");
            sb.AppendLine("                    return null;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            return sb;
        }
    }
}