using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Hotfix
{
    public class HotfixFunc
    {
        internal enum MethodType
        {
            Base,
            Hotfix,
        }

        internal readonly MethodType Type;
        internal readonly HotfixMethodInfo HotfixMethodInfo;
        internal readonly MethodInfo MethodInfo;

        internal readonly Type[] Parameters;
        internal int ParametersCount => Parameters.Length;
        internal readonly bool IsStatic;
        internal readonly Type ReturnType;

        internal HotfixFunc(HotfixMethodInfo methodInfo)
        {
            Type = MethodType.Hotfix;
            HotfixMethodInfo = methodInfo;
            Parameters = HotfixMethodInfo.Parameters.Select(p => TypeManager.GetType(p)).ToArray();
            IsStatic = methodInfo.IsStatic;
            ReturnType = TypeManager.GetType(methodInfo.ReturnType);
        }

        internal HotfixFunc(MethodInfo methodInfo)
        {
            Type = MethodType.Base;
            MethodInfo = methodInfo;
            Parameters = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            IsStatic = methodInfo.IsStatic;
            ReturnType = methodInfo.ReturnType;
        }

        public object Invoke(object obj, params object[] paras)
        {
            if (Type == MethodType.Base)
            {
                return MethodInfo.Invoke(obj, paras);
            }
            else
            {
                Debug.Log("执行热更逻辑:" + HotfixMethodInfo.Name);
                List<object> values = new List<object>(paras.Length + 1);
                values.Add(obj);
                values.AddRange(paras);
                return InvokeInternel(values);
            }
        }

        public void InvokeVoid(object obj, params object[] paras)
        {
            if (Type == MethodType.Base)
            {
                MethodInfo.Invoke(obj, paras);
            }
            else
            {
                Debug.Log("执行热更逻辑:" + HotfixMethodInfo.Name);
                List<object> values = new List<object>(paras.Length + 1);
                values.Add(obj);
                values.AddRange(paras);
                InvokeInternel(values);
            }
        }

        private object InvokeInternel(IReadOnlyList<object> paras)
        {
            object[] vars = new object[HotfixMethodInfo.Body.Variables.Length];
            for (int i = 0; i < HotfixMethodInfo.Body.Variables.Length; i++)
            {
                Type type = TypeManager.GetType(HotfixMethodInfo.Body.Variables[i]);
                vars[i] = type.IsValueType ? Activator.CreateInstance(type) : null;
            }

            Stack stack = new Stack();
            int instructionIndex = 0;
            while (true)
            {
                HotfixInstruction instruction = HotfixMethodInfo.Body.Instructions[instructionIndex];

                if (instruction.Code == HotfixOpCode.Ret)
                {
                    object result;
                    if (HotfixMethodInfo.ReturnType == typeof(void).ToString())
                        result = null;
                    else
                        result = stack.Pop();
                    return result;
                }
                else
                {
                    instructionIndex = InvokeOneCode(paras, vars, stack, instruction);
                }
            }
        }

        private int InvokeOneCode(IReadOnlyList<object> paras, object[] vars, Stack stack, HotfixInstruction instruction)
        {
            const BindingFlags bindingFlags =
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static;
            switch (instruction.Code)
            {
                case HotfixOpCode.Nop:
                    return instruction.NextOffset;
                case HotfixOpCode.Ldarg_0:
                    stack.Push(paras[0]);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldarg_1:
                    stack.Push(paras[1]);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldarg_2:
                    stack.Push(paras[2]);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldarg_3:
                    stack.Push(paras[2]);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldloc_0:
                    stack.Push(vars[0]);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldloc_1:
                    stack.Push(vars[1]);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldloc_2:
                    stack.Push(vars[2]);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldloc_3:
                    stack.Push(vars[3]);
                    return instruction.NextOffset;
                case HotfixOpCode.Stloc_0:
                    vars[0] = stack.Pop();
                    return instruction.NextOffset;
                case HotfixOpCode.Stloc_1:
                    vars[1] = stack.Pop();
                    return instruction.NextOffset;
                case HotfixOpCode.Stloc_2:
                    vars[2] = stack.Pop();
                    return instruction.NextOffset;
                case HotfixOpCode.Stloc_3:
                    vars[3] = stack.Pop();
                    return instruction.NextOffset;
                case HotfixOpCode.Ldarg_S:
                    {
                        int index = Convert.ToInt32(instruction.Operand);
                        stack.Push(paras[index]);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Ldloca_S:
                    stack.Push(vars[0]);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldnull:
                    stack.Push(null);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldc_I4_M1:
                    stack.Push(-1);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldc_I4_0:
                    stack.Push(0);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldc_I4_1:
                    stack.Push(1);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldc_I4_2:
                    stack.Push(2);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldc_I4_3:
                    stack.Push(3);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldc_I4_4:
                    stack.Push(4);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldc_I4_5:
                    stack.Push(5);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldc_I4_6:
                    stack.Push(6);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldc_I4_7:
                    stack.Push(7);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldc_I4_8:
                    stack.Push(8);
                    return instruction.NextOffset;
                case HotfixOpCode.Ldc_I4_S:
                    {
                        sbyte value = Convert.ToSByte(instruction.Operand);
                        stack.Push(value);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Dup:
                    {
                        object result = stack.Peek();
                        stack.Push(result);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Pop:
                    stack.Pop();
                    return instruction.NextOffset;
                case HotfixOpCode.Call:
                    {
                        string methodFullName = (string)instruction.Operand;
                        HotfixFunc method = HotfixRunner.Create(methodFullName.Split(' ')[1]);

                        object[] paraValues = new object[method.ParametersCount];
                        ParameterModifier[] paramMods = new ParameterModifier[1];
                        for (int i = 0; i < method.ParametersCount; i++)
                        {
                            object v = stack.Pop();
                            paraValues[method.ParametersCount - 1 - i] = v;
                        }

                        object obj = null;
                        if (!method.IsStatic)
                            obj = stack.Pop();

                        if (method.ReturnType == typeof(void))
                            method.InvokeVoid(obj, paraValues);
                        else
                        {
                            object result = method.Invoke(obj, paraValues);
                            stack.Push(result);
                        }

                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Br_S:
                    return Convert.ToInt32(instruction.Operand);
                case HotfixOpCode.Brfalse_S:
                    {
                        object value = stack.Pop();
                        bool flag =
                            value == null ||
                            value is int intValue && intValue == 0 ||
                            value is bool boolValue && boolValue == false;
                        if (flag)
                            return Convert.ToInt32(instruction.Operand);
                        else
                            return instruction.NextOffset;
                    }
                case HotfixOpCode.Brtrue_S:
                    {
                        object value = stack.Pop();
                        bool boolValue = value is not null || Convert.ToBoolean(value);
                        if (boolValue)
                            return Convert.ToInt32(instruction.Operand);
                        else
                            return instruction.NextOffset;
                    }
                case HotfixOpCode.Beq_S:
                    {
                        object v2 = stack.Pop();
                        object v1 = stack.Pop();
                        // v1==v2
                        var result = OPOperator.Equal(new OpValue(v1), new OpValue(v2)).BoolValue;
                        if (result)
                            return Convert.ToInt32(instruction.Operand);
                        else
                            return instruction.NextOffset;
                    }
                case HotfixOpCode.Bge_S:
                    {
                        object v2 = stack.Pop();
                        object v1 = stack.Pop();
                        // v1>=v2
                        var result = OPOperator.Less(new OpValue(v2), new OpValue(v1)).BoolValue;
                        if (result)
                            return Convert.ToInt32(instruction.Operand);
                        else
                            return instruction.NextOffset;
                    }
                case HotfixOpCode.Bgt_S:
                    {
                        object v2 = stack.Pop();
                        object v1 = stack.Pop();
                        // v1>v2
                        var result = OPOperator.Greater(new OpValue(v1), new OpValue(v2)).BoolValue;
                        if (result)
                            return Convert.ToInt32(instruction.Operand);
                        else
                            return instruction.NextOffset;

                    }
                case HotfixOpCode.Ble_S:
                    {
                        object v2 = stack.Pop();
                        object v1 = stack.Pop();
                        // v1<=v2
                        var result = OPOperator.Greater(new OpValue(v2), new OpValue(v1)).BoolValue;
                        if (result)
                            return Convert.ToInt32(instruction.Operand);
                        else
                            return instruction.NextOffset;
                    }
                case HotfixOpCode.Blt_S:
                    {
                        object v2 = stack.Pop();
                        object v1 = stack.Pop();
                        // v1<v2
                        var result = OPOperator.Less(new OpValue(v1), new OpValue(v2)).BoolValue;
                        if (result)
                            return Convert.ToInt32(instruction.Operand);
                        else
                            return instruction.NextOffset;
                    }
                case HotfixOpCode.Bne_Un_S:
                    {
                        object v2 = stack.Pop();
                        object v1 = stack.Pop();
                        var result = !OPOperator.Equal(new OpValue(v1), new OpValue(v2)).BoolValue;
                        if (result)
                            return Convert.ToInt32(instruction.Operand);
                        else
                            return instruction.NextOffset;
                    }
                case HotfixOpCode.Add:
                    {
                        object v2 = stack.Pop();
                        object v1 = stack.Pop();
                        var result = OPOperator.Add(new OpValue(v2), new OpValue(v1)).ObjectValue;
                        stack.Push(result);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Sub:
                    {
                        object v2 = stack.Pop();
                        object v1 = stack.Pop();
                        var result = OPOperator.Sub(new OpValue(v1), new OpValue(v2)).ObjectValue;
                        stack.Push(result);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Mul:
                    {
                        object v2 = stack.Pop();
                        object v1 = stack.Pop();
                        var result = OPOperator.Mul(new OpValue(v1), new OpValue(v2)).ObjectValue;
                        stack.Push(result);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Div:
                    {
                        object v2 = stack.Pop();
                        object v1 = stack.Pop();
                        object result = OPOperator.Div(new OpValue(v1), new OpValue(v2)).ObjectValue;
                        stack.Push(result);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.And:
                    {
                        int v2 = Convert.ToInt32(stack.Pop());
                        int v1 = Convert.ToInt32(stack.Pop());
                        int result = v1 & v2;
                        stack.Push(result);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Or:
                    {
                        int v2 = Convert.ToInt32(stack.Pop());
                        int v1 = Convert.ToInt32(stack.Pop());
                        int result = v1 | v2;
                        stack.Push(result);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Xor:
                    {
                        int v2 = Convert.ToInt32(stack.Pop());
                        int v1 = Convert.ToInt32(stack.Pop());
                        int result = v1 ^ v2;
                        stack.Push(result);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Shl:
                    {
                        int v2 = Convert.ToInt32(stack.Pop());
                        int v1 = Convert.ToInt32(stack.Pop());
                        int result = v1 << v2;
                        stack.Push(result);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Shr:
                    {
                        int v2 = Convert.ToInt32(stack.Pop());
                        int v1 = Convert.ToInt32(stack.Pop());
                        int result = v1 >> v2;
                        stack.Push(result);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Conv_I4:
                    {
                        object value = stack.Pop();
                        int intValue = Convert.ToInt32(value);
                        stack.Push(intValue);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Callvirt:
                    goto case HotfixOpCode.Call;
                case HotfixOpCode.Ldstr:
                    stack.Push((string)instruction.Operand);
                    return instruction.NextOffset;
                case HotfixOpCode.Newobj:
                    {
                        string methodFullName = (string)instruction.Operand;
                        Regex regex = new Regex("^(\\S*) (\\S*)::(\\S*)\\((\\S*)\\)$");
                        Match match = regex.Match(methodFullName);
                        Type type = TypeManager.GetType(match.Groups[2].Value);
                        Type[] paraTypes = match.Groups[4].Value
                            .Split(',')
                            .Where(name => !string.IsNullOrEmpty(name))
                            .Select(name => TypeManager.GetType(name))
                            .ToArray();
                        object[] paraValues = new object[paraTypes.Length];
                        for (int i = 0; i < paraTypes.Length; i++)
                        {
                            paraValues[paraTypes.Length - 1 - i] = stack.Pop();
                        }
                        ConstructorInfo constructor = type.GetConstructor(paraTypes);
                        var result = constructor.Invoke(paraValues);
                        stack.Push(result);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Castclass:
                    {
                        string typeName = (string)instruction.Operand;
                        Type type = TypeManager.GetType(typeName);

                        object value = stack.Pop();
                        value = Convert.ChangeType(value, type);
                        stack.Push(value);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Ldfld:
                    {
                        string fieldFullName = (string)instruction.Operand;
                        Regex regex = new Regex("^(\\S*) (\\S*)::(\\S*)$");
                        Match match = regex.Match(fieldFullName);
                        Type type = TypeManager.GetType(match.Groups[2].Value);
                        string fieldName = match.Groups[3].Value;
                        FieldInfo fieldInfo = type.GetField(fieldName, bindingFlags);
                        object value = fieldInfo.GetValue(stack.Pop());
                        stack.Push(value);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Ldflda:
                    goto case HotfixOpCode.Ldfld;
                case HotfixOpCode.Stfld:
                    {
                        string fieldFullName = (string)instruction.Operand;
                        Regex regex = new Regex("^(\\S*) (\\S*)::(\\S*)$");
                        Match match = regex.Match(fieldFullName);
                        Type type = TypeManager.GetType(match.Groups[2].Value);
                        string fieldName = match.Groups[3].Value;
                        FieldInfo fieldInfo = type.GetField(fieldName, bindingFlags);
                        object value = stack.Pop();
                        object obj = stack.Pop();
                        fieldInfo.SetValue(obj, value);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Ldsfld:
                    {
                        string fieldFullName = (string)instruction.Operand;
                        Regex regex = new Regex("^(\\S*) (\\S*)::(\\S*)$");
                        Match match = regex.Match(fieldFullName);
                        Type type = TypeManager.GetType(match.Groups[2].Value);
                        string fieldName = match.Groups[3].Value;
                        FieldInfo fieldInfo = type.GetField(fieldName, bindingFlags);
                        object value = fieldInfo.GetValue(null);
                        stack.Push(value);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Stsfld:
                    {
                        string fieldFullName = (string)instruction.Operand;
                        Regex regex = new Regex("^(\\S*) (\\S*)::(\\S*)$");
                        Match match = regex.Match(fieldFullName);
                        Type type = TypeManager.GetType(match.Groups[2].Value);
                        string fieldName = match.Groups[3].Value;
                        FieldInfo fieldInfo = type.GetField(fieldName, bindingFlags);
                        fieldInfo.SetValue(null, stack.Pop());
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Box:
                    // TODO:stack中不保存object，而是OpValue，以应对装箱拆箱操作
                    // 装箱指令不需要单独操作
                    return instruction.NextOffset;
                case HotfixOpCode.Ldlen:
                    {
                        object arr = stack.Pop();
                        int len = (arr as Array).Length;
                        stack.Push(len);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Ldelem_I4:
                    {
                        int index = Convert.ToInt32(stack.Pop());
                        Array arr = stack.Pop() as Array;
                        object item = arr.GetValue(index);
                        stack.Push(item);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Leave_S:
                    return Convert.ToInt32(instruction.Operand);
                case HotfixOpCode.Ldftn:
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
                        MethodInfo method = type.GetMethod(methodName, bindingFlags, null, paraTypes, null);
                        stack.Push(method.MethodHandle.GetFunctionPointer());
                        return instruction.NextOffset;
                    }
                default: throw new Exception("未识别的IL指令:" + instruction.Code.ToString());
            }
        }
    }
}
