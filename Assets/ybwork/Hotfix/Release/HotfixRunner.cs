using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Hotfix
{
    public static class HotfixRunner
    {
        public static readonly string RootPath = Path.Combine(Application.persistentDataPath, "hotfix");
        public static HotfixFunc Create(string content)
        {
            HotfixMethodInfo method = JsonConvert.DeserializeObject<HotfixMethodInfo>(content);
            return new HotfixFunc(method);
        }

        public static T Run<T>(StackTrace stackTrace, object obj)
        {
            MethodBase method = stackTrace.GetFrame(0).GetMethod();
            string name = method.DeclaringType.FullName + "." + method.Name;
            string path = Path.Combine(RootPath, name + ".json");
            string content = File.ReadAllText(path);

            HotfixFunc func = Create(content);

            object result = func.Invoke(obj, 2, 3);
            return (T)result;
        }

        public static void RunVoid(StackTrace stackTrace, object obj)
        {
            MethodBase method = stackTrace.GetFrame(0).GetMethod();
        }

        public static bool IsHotfixMethod(StackTrace stackTrace)
        {
            if (Application.isEditor)
                return false;

            MethodBase method = stackTrace.GetFrame(0).GetMethod();
            string name = method.DeclaringType.FullName + "." + method.Name;
            FileInfo file = new FileInfo(Path.Combine(RootPath, name + ".json"));
            return file.Exists;
        }
    }
}
