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
        private static readonly List<Assembly> _assemblies = new();
        private static readonly Dictionary<string, Type> _types = new();

        static TypeManager()
        {
            _assemblies.Add(typeof(object).Assembly);
            _assemblies.Add(typeof(System.Linq.Enumerable).Assembly);
            _assemblies.Add(typeof(List<>).Assembly);
            _assemblies.Add(typeof(TypeManager).Assembly);
            _assemblies.Add(typeof(MonoBehaviour).Assembly);
        }

        public static Type GetType(string name)
        {
            if (_types.TryGetValue(name, out Type type))
                return type;

            if (name.Contains('<') && !name.Contains("<>"))
                return GetGenericType(name);

            StackTrace stackTrace = new();
            foreach (StackFrame frame in stackTrace.GetFrames())
            {
                Assembly entryAssembly = frame.GetMethod().DeclaringType.Assembly;
                if (!_assemblies.Contains(entryAssembly))
                    _assemblies.Add(entryAssembly);
            }

            foreach (var assembly in _assemblies)
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
