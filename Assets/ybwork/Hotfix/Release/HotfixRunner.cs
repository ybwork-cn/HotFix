using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Hotfix
{
    public static class HotfixRunner
    {
        public static string RootPath => Application.persistentDataPath + "/hotfix";
        public static readonly Dictionary<string, HotfixMethodInfo> catalogue = new();

        public static IEnumerator InitAsync()
        {
            var request = UnityWebRequest.Get(RootPath + "/catalogue.json");
            yield return request.SendWebRequest();
            List<string> methods = JsonConvert.DeserializeObject<List<string>>(request.downloadHandler.text);

            foreach (string methodName in methods)
            {
                request = UnityWebRequest.Get($"{RootPath}/{methodName}.json");
                yield return request.SendWebRequest();
                string content = request.downloadHandler.text;
                HotfixMethodInfo method = JsonConvert.DeserializeObject<HotfixMethodInfo>(content);
                catalogue.Add(methodName, method);
            }
        }

        public static HotfixFunc Create(string name)
        {
            return new HotfixFunc(catalogue[name]);
        }

        public static T Run<T>(StackTrace stackTrace, object obj, params object[] paras)
        {
            MethodBase method = stackTrace.GetFrame(0).GetMethod();
            string name = method.DeclaringType.FullName + "." + method.Name;

            HotfixFunc func = Create(name);

            object result = func.Invoke(obj, paras);
            return (T)result;
        }

        public static void RunVoid(StackTrace stackTrace, object obj, params object[] paras)
        {
            MethodBase method = stackTrace.GetFrame(0).GetMethod();
            string name = method.DeclaringType.FullName + "." + method.Name;

            HotfixFunc func = Create(name);

            func.InvokeVoid(obj, paras);
        }

        public static bool IsHotfixMethod(StackTrace stackTrace)
        {
            if (Application.isEditor)
                return false;

            MethodBase method = stackTrace.GetFrame(0).GetMethod();
            string name = method.DeclaringType.FullName + "." + method.Name;
            return catalogue.ContainsKey(name);
        }
    }
}
