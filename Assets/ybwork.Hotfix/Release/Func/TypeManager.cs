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
            _assemblies.Add(typeof(Newtonsoft.Json.JsonConvert).Assembly);
        }

        public static Type GetType(string name)
        {
            name = name.Replace("/", "+");
            if (_types.TryGetValue(name, out Type type))
                return type;

            if (name.Contains('<') && !name.Contains("+<") && !name.Contains("<>"))
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
            if (!name.EndsWith(">"))
                return Array.Empty<Type>();

            while (!name.StartsWith("<"))
                name = name[1..];
            name = name[1..^1];

            List<Type> types = new List<Type>();
            int start = 0;
            int rest_arrow = 0;
            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] == '<')
                    rest_arrow++;
                else if (name[i] == '>')
                    rest_arrow--;
                else if (rest_arrow == 0 && name[i] == ',')
                {
                    types.Add(GetType(name[start..i]));
                    start = i + 1;
                    i++;
                }
            }
            if (start != name.Length)
                types.Add(GetType(name[start..]));
            return types.ToArray();
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
            Type[] paramTypes = GetGenericParamTypes($"<{match.Groups[2].Value}>");
            Type type = GetType(match.Groups[1].Value);
            type = type.MakeGenericType(paramTypes);
            _types[name] = type ?? throw new Exception(name);
            return type;
        }

        public static string GetFunctionName(string methodName)
        {
            if (!methodName.EndsWith(">"))
                return methodName;

            int count = 0;
            for (int i = 0; i < methodName.Length; i++)
            {
                if (methodName[i] == '>')
                    count++;
                else if (methodName[i] == '<')
                    count--;
                if (count == 0)
                    return methodName[..i];
            }
            throw new Exception("格式错误" + methodName);
        }
    }
}
