using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

namespace Hotfix
{
    public class HotfixFunc
    {
        internal readonly HotfixMethodInfo MethodInfo;

        internal HotfixFunc(HotfixMethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
        }

        public object Invoke(object obj, params object[] paras)
        {
            List<object> values = new List<object>(paras.Length + 1);
            values.Add(obj);
            values.AddRange(paras);
            return InvokeInternel(values);
        }

        private object InvokeInternel(IReadOnlyList<object> paras)
        {
            object[] vars = new object[MethodInfo.Body.Variables.Length];
            Stack stack = new Stack();
            int instructionIndex = 0;
            while (true)
            {
                HotfixInstruction instruction = MethodInfo.Body.Instructions[instructionIndex];
                switch (instruction.Code)
                {
                    case HotfixOpCode.Ldarg_0:
                        stack.Push(GetParamValue(paras, 0));
                        instructionIndex++;
                        break;
                    case HotfixOpCode.Ldarg_1:
                        stack.Push(GetParamValue(paras, 1));
                        instructionIndex++;
                        break;
                    case HotfixOpCode.Ldarg_2:
                        stack.Push(GetParamValue(paras, 2));
                        instructionIndex++;
                        break;
                    case HotfixOpCode.Ldarg_S:
                        {
                            int index = Convert.ToInt32(instruction.Operand);
                            stack.Push(GetParamValue(paras, index));
                            instructionIndex++;
                            break;
                        }
                    case HotfixOpCode.Ldnull:
                        {
                            stack.Push(null);
                            instructionIndex++;
                            break;
                        }
                    case HotfixOpCode.Dup:
                        {
                            object result = stack.Peek();
                            stack.Push(result);
                            instructionIndex++;
                            break;
                        }
                    case HotfixOpCode.Call:
                        {
                            string methodFullName = (string)instruction.Operand;
                            Regex regex = new Regex("^(\\S*) (\\S*)::(\\S*)\\((\\S*)\\)$");
                            Match match = regex.Match(methodFullName);
                            Type type = TypeManager.GetType(match.Groups[2].Value);
                            string methodName = match.Groups[3].Value;
                            Type[] paraTypes = match.Groups[4].Value
                                .Split(',')
                                .Where(name => !string.IsNullOrEmpty(name))
                                .Select(name => TypeManager.GetType(name))
                                .ToArray();
                            MethodInfo method = type.GetMethod(methodName, paraTypes);
                            object[] paraValues = new object[paraTypes.Length];
                            for (int i = 0; i < paraTypes.Length; i++)
                            {
                                paraValues[paraTypes.Length - 1 - i] = stack.Pop();
                            }
                            object obj = null;
                            if (!method.IsStatic)
                                obj = stack.Pop();
                            if (method.ReturnType == typeof(void))
                                method.Invoke(obj, paraValues);
                            else
                            {
                                object result = method.Invoke(obj, paraValues);
                                stack.Push(result);
                            }
                            instructionIndex++;
                            break;
                        }
                    case HotfixOpCode.Ret:
                        {
                            object result = stack.Pop();
                            return result;
                        }
                    case HotfixOpCode.Add:
                        {
                            object v2 = stack.Pop();
                            object v1 = stack.Pop();
                            var result = OPOperator.Add(new OpValue(v2), new OpValue(v1)).ObjectValue;
                            stack.Push(result);
                            instructionIndex++;
                            break;
                        }
                    case HotfixOpCode.Sub:
                        {
                            object v2 = stack.Pop();
                            object v1 = stack.Pop();
                            var result = OPOperator.Sub(new OpValue(v1), new OpValue(v2)).ObjectValue;
                            stack.Push(result);
                            instructionIndex++;
                            break;
                        }
                    case HotfixOpCode.Box:
                        {
                            // TODO:stack中不保存object，而是OpValue，以应对装箱拆箱操作
                            // 装箱指令不需要单独操作
                            instructionIndex++;
                            break;
                        }
                    default: throw new Exception("未识别的IL指令:" + instruction.Code.ToString());
                }
            }
        }

        private object GetParamValue(IReadOnlyList<object> paras, int index)
        {
            if (MethodInfo.IsStatic)
                return index;
            else
                return index + 1;
        }

        public void InvokeVoid(params object[] paras)
        {
            return;
        }
    }
}
