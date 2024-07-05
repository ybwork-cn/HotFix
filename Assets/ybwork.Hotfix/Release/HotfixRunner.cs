using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
            Dictionary<string, string> methods = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);

            foreach (var item in methods)
            {
                request = UnityWebRequest.Get($"{RootPath}/{item.Value}.json");
                yield return request.SendWebRequest();
                string content = request.downloadHandler.text;
                HotfixMethodInfo method = JsonConvert.DeserializeObject<HotfixMethodInfo>(content);
                _catalogue.Add(item.Key.Split(' ')[1], method);
            }
        }

        public static HotfixFunc Create(string methodFullName)
        {
            if (TryGetHotfixMethod(methodFullName, out HotfixMethodInfo methodInfo))
            {
                return new HotfixFunc(methodInfo);
            }
            else
            {
                const BindingFlags bindingFlags =
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.Static;

                Regex regex = new Regex("^(\\S*)::(\\S*)\\((\\S*)\\)$");
                Match match = regex.Match(methodFullName);

                string typeName = match.Groups[1].Value;
                string methodName = match.Groups[2].Value;
                string paraTypeNameString = match.Groups[3].Value;

                Type[] methodGenericTypeArguments = TypeManager.GetGenericParamTypes(methodName);
                for (int i = 0; i < methodGenericTypeArguments.Length; i++)
                {
                    string oldName = $"!!{i}";
                    string newValue = methodGenericTypeArguments[i].FullName;
                    paraTypeNameString = paraTypeNameString.Replace(oldName, newValue);
                }

                Type type = TypeManager.GetType(typeName);
                for (int i = 0; i < type.GenericTypeArguments.Length; i++)
                {
                    string oldName = $"!{i}";
                    string newValue = type.GenericTypeArguments[i].FullName;
                    paraTypeNameString = paraTypeNameString.Replace(oldName, newValue);
                }

                if (methodGenericTypeArguments.Length > 0)
                {
                    methodName = TypeManager.GetFunctionName(methodName);

                    MethodInfo method = type.GetMethods(bindingFlags)
                        .Where(func => func.Name == methodName)
                        .Where(func => func.IsGenericMethod)
                        .Where(Func => Func.GetGenericArguments().Length == methodGenericTypeArguments.Length)
                        .First()
                        .MakeGenericMethod(methodGenericTypeArguments);
                    return new HotfixFunc(method, method.ReturnType);
                }
                else
                {
                    Type[] paraTypes = TypeManager.GetGenericParamTypes($"<{paraTypeNameString}>");
                    if (methodName == ".ctor")
                    {
                        ConstructorInfo method = type.GetConstructor(paraTypes);
                        return new HotfixFunc(method, typeof(void));
                    }
                    else
                    {
                        MethodInfo method = type.GetMethod(methodName, bindingFlags, null, paraTypes, null);
                        return new HotfixFunc(method, method.ReturnType);
                    }
                }
            }
        }

        public static HotfixFunc Create(MethodBase method)
        {
            string paramTypesString = TypeManager.GetString(method.GetParameters().Select(p => p.ParameterType));

            string methodFullName = $"{TypeManager.GetString(method.DeclaringType)}::{method.Name}({paramTypesString})";

            return Create(methodFullName);
        }

        public static T Run<T>(StackTrace stackTrace, object obj, params object[] paras)
        {
            MethodBase method = stackTrace.GetFrame(0).GetMethod();

            HotfixFunc func = Create(method);
            object result = func.Invoke(obj, paras);
            return (T)result;
        }

        public static void RunVoid(StackTrace stackTrace, object obj, params object[] paras)
        {
            MethodBase method = stackTrace.GetFrame(0).GetMethod();

            HotfixFunc func = Create(method);
            func.InvokeVoid(obj, paras);
        }

        public static bool IsHotfixMethod(StackTrace stackTrace)
        {
            //if (Application.isEditor)
            //    return false;

            MethodBase method = stackTrace.GetFrame(0).GetMethod();
            string parameters = TypeManager.GetString(method.GetParameters().Select(p => p.ParameterType));
            string methodName = $"{TypeManager.GetString(method.DeclaringType)}::{method.Name}({parameters})";
            return TryGetHotfixMethod(methodName, out _);
        }

        private static bool TryGetHotfixMethod(string methodName, out HotfixMethodInfo methodInfo)
        {
            return _catalogue.TryGetValue(methodName, out methodInfo);
        }
    }
}
