using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using ybwork.Async;

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
            _assemblies.Add(typeof(YueTask).Assembly);
        }

        public static Type GetType(string name)
        {
            if (_types.TryGetValue(name, out Type type))
                return type;

            if (name.Contains('<') && !name.Contains("/<") && !name.Contains("<>"))
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

            _types[name] = type ?? throw new Exception(name);
            return type;
        }

        public static Type[] GetGenericParamTypes(string name)
        {
            if (!name.Contains("<"))
                return Array.Empty<Type>();

            while (!name.StartsWith("<"))
                name = name[1..];
            name = name[1..^1];

            Type[] paramTypes = name.Split(',')
                .Select(paramTypeName => GetType(paramTypeName))
                .ToArray();
            return paramTypes;
        }

        public static string GetString(Type type)
        {
            string name = type.Name;
            if (!string.IsNullOrEmpty(type.Namespace))
                name = type.Namespace + "." + name;

            if (!type.IsGenericType)
                return name;

            string genericArgumentsString = GetString(type.GetGenericArguments());
            return name + "<" + genericArgumentsString + ">";
        }

        public static string GetString(IEnumerable<Type> types)
        {
            return string.Join(",", types.Select(p => GetString(p)));
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
            _types[name] = type ?? throw new Exception(name);
            return type;
        }
    }
}
