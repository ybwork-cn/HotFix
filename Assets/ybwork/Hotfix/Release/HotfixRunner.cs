using Newtonsoft.Json;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Hotfix
{
    public class HotfixRunner
    {
        public static HotfixFunc Create(string content)
        {
            HotfixMethodInfo method = JsonConvert.DeserializeObject<HotfixMethodInfo>(content);
            return new HotfixFunc(method);
        }

        public static T Run<T>(object obj)
        {
            System.Diagnostics.StackTrace stackTrace = new();
            var method = stackTrace.GetFrame(1).GetMethod();

            string content = File.ReadAllText(Application.streamingAssetsPath + "/aa.json");
            HotfixFunc func = Create(content);
            object result = func.Invoke(obj, 2, 3);

            return (T)result;
        }

        public static void RunVoid(object obj)
        {
            System.Diagnostics.StackTrace stackTrace = new();
            var method = stackTrace.GetFrame(1).GetMethod();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool IsHotfixMethod()
        {
            System.Diagnostics.StackTrace stackTrace = new();
            var method = stackTrace.GetFrame(1).GetMethod();
            return method.DeclaringType.FullName == "Main" && method.Name == "Add";
        }
    }
}
