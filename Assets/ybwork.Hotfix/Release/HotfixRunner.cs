using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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

                string typeName = match.Groups[1].Value;
                string methodName = match.Groups[2].Value;
                string paraTypeNameString = match.Groups[3].Value;

                Type type = TypeManager.GetType(typeName);

                for (int i = 0; i < type.GenericTypeArguments.Length; i++)
                {
                    string oldName = $"!{i}";
                    string newValue = type.GenericTypeArguments[i].FullName;
                    paraTypeNameString = paraTypeNameString.Replace(oldName, newValue);
                }

                if (methodName.Contains("<"))
                {
                    Type[] genericTypeTypes = TypeManager.GetGenericParamTypes(methodName);

                    for (int i = 0; i < genericTypeTypes.Length; i++)
                    {
                        string oldName = $"!!{i}";
                        string newValue = genericTypeTypes[i].FullName;
                        paraTypeNameString = paraTypeNameString.Replace(oldName, newValue);
                    }
                    Type[] paraTypes = TypeManager.GetGenericParamTypes($"<{paraTypeNameString}>");

                    methodName = methodName[..methodName.IndexOf("<")];

                    const BindingFlags bindingFlags =
                        BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance | BindingFlags.Static;
                    MethodInfo method = type.GetMethods(bindingFlags)
                        .Where(func => func.Name == methodName)
                        .Where(func => func.IsGenericMethod)
                        .Where(Func => Func.GetGenericArguments().Length == genericTypeTypes.Length)
                        .First()
                        .MakeGenericMethod(genericTypeTypes);
                    return new HotfixFunc(method);
                }
                else
                {
                    Type[] paraTypes = TypeManager.GetGenericParamTypes($"<{paraTypeNameString}>");
                    MethodInfo method = type.GetMethod(methodName, paraTypes);
                    return new HotfixFunc(method);
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
            return IsHotfixMethod(methodName, out _);
        }

        private static bool IsHotfixMethod(string methodName, out HotfixMethodInfo methodInfo)
        {
            return _catalogue.TryGetValue(methodName, out methodInfo);
        }
    }
}
