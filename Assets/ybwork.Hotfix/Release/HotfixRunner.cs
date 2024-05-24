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
            if (IsHotfixMethod(methodFullName, out HotfixMethodInfo methodInfo))
            {
                return new HotfixFunc(methodInfo);
            }
            else
            {
                Regex regex = new Regex("^(\\S*)::(\\S*)\\((\\S*)\\)$");
                Match match = regex.Match(methodFullName);
                Type type = TypeManager.GetType(match.Groups[1].Value);
                string methodName = match.Groups[2].Value;
                Type[] genericTypeTypes = TypeManager.GetGenericParamTypes(methodName);
                string[] paraTypeNames = match.Groups[3].Value
                    .Split(',')
                    .ToArray();
                Type[] paraTypes = paraTypeNames
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Select(name =>
                    {
                        if (!name.StartsWith('!'))
                            return TypeManager.GetType(name);
                        else
                        {
                            while (name.StartsWith("!"))
                                name = name[1..];
                            // ref参数
                            string genericTypeIndexStr = name.EndsWith("&")
                                ? name[..^1]
                                : name[..];
                            int genericTypeIndex = int.Parse(genericTypeIndexStr);
                            return genericTypeTypes[genericTypeIndex];
                        }
                    })
                    .ToArray();

                if (methodName.Contains("<"))
                    methodName = methodName[..methodName.IndexOf("<")];

                MethodInfo method;
                if (genericTypeTypes.Length > 0)
                {
                    const BindingFlags bindingFlags =
                        BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance | BindingFlags.Static;
                    method = type.GetMethods(bindingFlags)
                        .Where(func => func.Name == methodName)
                        .Where(func => func.IsGenericMethod)
                        .Where(Func => Func.GetGenericArguments().Length == genericTypeTypes.Length)
                        .First()
                        .MakeGenericMethod(genericTypeTypes);
                }
                else
                    method = type.GetMethod(methodName, paraTypes);
                return new HotfixFunc(method);
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
            return IsHotfixMethod(methodName, out _);
        }

        private static bool IsHotfixMethod(string methodName, out HotfixMethodInfo methodInfo)
        {
            return _catalogue.TryGetValue(methodName, out methodInfo);
        }
    }
}
