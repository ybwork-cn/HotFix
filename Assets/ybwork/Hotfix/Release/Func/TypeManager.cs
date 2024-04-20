using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Hotfix
{
    internal static class TypeManager
    {
        private static readonly List<Assembly> Assemblies = new();
        private static readonly Dictionary<string, Type> Types = new();

        static TypeManager()
        {
            Assemblies.Add(typeof(object).Assembly);
            Assemblies.Add(typeof(System.Linq.Enumerable).Assembly);
            Assemblies.Add(typeof(List<>).Assembly);
            Assemblies.Add(typeof(IEnumerable<>).Assembly);
            Assemblies.Add(typeof(TypeManager).Assembly);
            Assemblies.Add(typeof(MonoBehaviour).Assembly);
        }

        public static Type GetType(string name)
        {
            if (Types.TryGetValue(name, out Type type))
                return type;

            if (name.Contains('<'))
                return GetGenericType(name);

            StackTrace stackTrace = new();
            StackFrame frame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
            Assembly entryAssembly = frame.GetMethod().DeclaringType.Assembly;
            if (!Assemblies.Contains(entryAssembly))
                Assemblies.Add(entryAssembly);

            foreach (var assembly in Assemblies)
            {
                var types = assembly.GetTypes();
                type = assembly.GetType(name);
                if (type != null)
                    break;
            }
            if (type == null)
                throw new Exception(name);
            return type;
        }

        private static Type GetGenericType(string name)
        {
            Regex regex = new Regex("^(\\S+`\\d+)<(\\S+)>$");
            Match match = regex.Match(name);
            string typeName = match.Groups[1].Value;
            Type[] paramTypes = match.Groups[2].Value.Split(',')
                .Select(paramTypeName => GetType(paramTypeName))
                .ToArray();
            Type type = GetType(typeName);
            type = type.MakeGenericType(paramTypes);
            return type;
        }
    }
}
