using Newtonsoft.Json;
using System;
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
        private static readonly Dictionary<string, HotfixMethodInfo> _catalogue = new();

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
                _catalogue.Add(methodName, method);
            }
        }

        public static HotfixFunc Create(string name)
        {
            return new HotfixFunc(_catalogue[name]);
        }

        public static T Run<T>(StackTrace stackTrace, object obj, params object[] paras)
        {
            MethodBase method = stackTrace.GetFrame(0).GetMethod();
            string name = method.DeclaringType.FullName + "." + method.Name;

            HotfixFunc func = Create(name);
            Debug.Log("执行热更逻辑:" + name);
            object result = func.Invoke(obj, paras);
            return (T)result;
        }

        public static object Run(HotfixMethodInfo methodInfo, object obj, params object[] paras)
        {
            HotfixFunc func = new HotfixFunc(methodInfo);
            object result = func.Invoke(obj, paras);
            return result;
        }

        public static void RunVoid(StackTrace stackTrace, object obj, params object[] paras)
        {
            MethodBase method = stackTrace.GetFrame(0).GetMethod();
            string name = method.DeclaringType.FullName + "." + method.Name;

            HotfixFunc func = Create(name);
            Debug.Log("执行热更逻辑:" + name);
            func.InvokeVoid(obj, paras);
        }

        public static void RunVoid(HotfixMethodInfo methodInfo, object obj, params object[] paras)
        {
            HotfixFunc func = new HotfixFunc(methodInfo);
            func.InvokeVoid(obj, paras);
        }

        public static bool IsHotfixMethod(StackTrace stackTrace)
        {
            //if (Application.isEditor)
            //    return false;

            MethodBase method = stackTrace.GetFrame(0).GetMethod();
            return IsHotfixMethod(method.ReflectedType, method.Name, out _);
        }

        public static bool IsHotfixMethod(Type type, string methodName, out HotfixMethodInfo methodInfo)
        {
            string name = type.FullName + "." + methodName;
            return _catalogue.TryGetValue(name, out methodInfo);
        }
    }
}
