using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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
            Assemblies.Add(typeof(TypeManager).Assembly);
            Assemblies.Add(typeof(MonoBehaviour).Assembly);
        }
        public static Type GetType(string name)
        {
            if (Types.TryGetValue(name, out Type type))
                return type;

            StackTrace stackTrace = new();
            StackFrame frame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
            Assembly entryAssembly = frame.GetMethod().DeclaringType.Assembly;
            if (!Assemblies.Contains(entryAssembly))
                Assemblies.Add(entryAssembly);

            foreach (var assembly in Assemblies)
            {
                type = assembly.GetType(name);
                if (type != null)
                    break;
            }
            if (type == null)
                throw new Exception(name);
            return type;
        }
    }
}
