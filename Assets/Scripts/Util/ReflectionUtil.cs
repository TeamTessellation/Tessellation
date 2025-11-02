using System;
using System.Collections.Generic;
using System.Reflection;

namespace Util
{
    public static class ReflectionUtil
    {
        
        enum AssemblyType {
            AssemblyCSharp,
            AssemblyCSharpEditor,
            AssemblyCSharpEditorFirstPass,
            AssemblyCSharpFirstPass
        }
        
        static AssemblyType? GetAssemblyType(string assemblyName) {
            return assemblyName switch {
                "Assembly-CSharp" => AssemblyType.AssemblyCSharp,
                "Assembly-CSharp-Editor" => AssemblyType.AssemblyCSharpEditor,
                "Assembly-CSharp-Editor-firstpass" => AssemblyType.AssemblyCSharpEditorFirstPass,
                "Assembly-CSharp-firstpass" => AssemblyType.AssemblyCSharpFirstPass,
                _ => null
            };
        }
        
        static void AddTypesFromAssembly(Type[] assemblyTypes, Type parentType, ICollection<Type> results) {
            if (assemblyTypes == null) return;
            
            // 일반 타입인 경우
            if (!parentType.IsGenericTypeDefinition)
            {
                for (int i = 0; i < assemblyTypes.Length; i++) {
                    Type type = assemblyTypes[i];
                    if (type != parentType && parentType.IsAssignableFrom(type)) {
                        results.Add(type);
                    }
                }
            }
            // 오픈 제네릭 타입인 경우
            else
            {
                for (int i = 0; i < assemblyTypes.Length; i++)
                {
                    Type type = assemblyTypes[i];
                    if (type != parentType && IsSubclassOfRawGeneric(type, parentType))
                    {
                        results.Add(type);
                    }
                }
            }
        }
        
        public static List<Type> GetTypes(Type parentType) {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            Dictionary<AssemblyType, Type[]> assemblyTypes = new Dictionary<AssemblyType, Type[]>();
            List<Type> types = new List<Type>();
            for (int i = 0; i < assemblies.Length; i++) {
                AssemblyType? assemblyType = GetAssemblyType(assemblies[i].GetName().Name);
                if (assemblyType != null) {
                    assemblyTypes.Add((AssemblyType) assemblyType, assemblies[i].GetTypes());
                }
            }
            
            assemblyTypes.TryGetValue(AssemblyType.AssemblyCSharp, out var assemblyCSharpTypes);
            AddTypesFromAssembly(assemblyCSharpTypes, parentType, types);

            assemblyTypes.TryGetValue(AssemblyType.AssemblyCSharpFirstPass, out var assemblyCSharpFirstPassTypes);
            AddTypesFromAssembly(assemblyCSharpFirstPassTypes, parentType, types);
            
            return types;
        }
        
        /// <summary>
        /// 오픈 제네릭 타입의 서브클래스인지 확인합니다.
        /// </summary>
        /// <param name="toCheck"></param>
        /// <param name="openGeneric"></param>
        /// <returns></returns>
        private static bool IsSubclassOfRawGeneric(Type toCheck, Type openGeneric)
        {
            // 베이스타입 체인 따라가면서 검사
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (cur == openGeneric)
                    return true;
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}