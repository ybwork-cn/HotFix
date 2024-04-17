using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Hotfix
{
    public class HotfixRunner
    {
        public static HotfixMethodInfo Create(string content)
        {
            HotfixMethodInfo method = JsonConvert.DeserializeObject<HotfixMethodInfo>(content);
            return method;
        }

        public static T Run<T>()
        {
            return default;
        }

        public static void RunVoid()
        {
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
