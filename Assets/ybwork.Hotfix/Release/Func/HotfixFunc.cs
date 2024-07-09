using Newtonsoft.Json;
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
        internal readonly MethodBase MethodBase;

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

        internal HotfixFunc(MethodBase methodBase, Type returnType)
        {
            Type = MethodType.Base;
            MethodBase = methodBase;
            Parameters = methodBase.GetParameters().Select(p => p.ParameterType).ToArray();
            IsStatic = methodBase.IsStatic;
            ReturnType = returnType;
        }

        public object Invoke(object obj, params object[] paras)
        {
            if (Type == MethodType.Base)
            {
                return MethodBase.Invoke(obj, paras);
            }
            else
            {
                Debug.Log("执行热更逻辑:" + HotfixMethodInfo.Name);
                int parasCount = IsStatic ? paras.Length : paras.Length + 1;
                List<object> values = new List<object>(parasCount);
                if (!IsStatic)
                    values.Add(obj);
                values.AddRange(paras);
                return InvokeInternel(values);
            }
        }

        public void InvokeVoid(object obj, params object[] paras)
        {
            if (Type == MethodType.Base)
            {
                MethodBase.Invoke(obj, paras);
            }
            else
            {
                Debug.Log("执行热更逻辑:" + HotfixMethodInfo.Name);
                int parasCount = IsStatic ? paras.Length : paras.Length + 1;
                List<object> values = new List<object>(parasCount);
                if (!IsStatic)
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
                    {
                        int index = Convert.ToInt32(instruction.Operand);
                        stack.Push(vars[index]);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Stloc_S:
                    {
                        int index = Convert.ToInt32(instruction.Operand);
                        vars[index] = stack.Pop();
                        return instruction.NextOffset;
                    }
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
                        stack.Push((int)value);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Ldc_I4:
                    {
                        int value = Convert.ToInt32(instruction.Operand);
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
                        for (int i = 0; i < method.ParametersCount; i++)
                        {
                            object v = stack.Pop();
                            paraValues[method.ParametersCount - 1 - i] = v;
                        }

                        object obj = method.IsStatic ? null : stack.Pop();

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
                case HotfixOpCode.Switch:
                    {
                        int[] instructionOffsets = JsonConvert.DeserializeObject<int[]>((string)instruction.Operand);
                        int index = (int)stack.Pop();
                        return instructionOffsets[index];
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

                        // 如果是创建某个委托的实例，则跳过
                        // 因为上一步的HotfixOpCode.Ldftn已经创建出了最终的委托
                        if (typeof(Delegate).IsAssignableFrom(type))
                        {
                            return instruction.NextOffset;
                        }

                        Type[] paraTypes = TypeManager.GetGenericParamTypes($"<{match.Groups[4].Value}>");
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
                case HotfixOpCode.Ldelem_Ref:
                    goto case HotfixOpCode.Ldelem_I4;
                case HotfixOpCode.Ldtoken:
                    {
                        string typeName = (string)instruction.Operand;
                        RuntimeTypeHandle type = TypeManager.GetType(typeName).TypeHandle;
                        stack.Push(type);
                        return instruction.NextOffset;
                    }
                case HotfixOpCode.Leave_S:
                    return Convert.ToInt32(instruction.Operand);
                case HotfixOpCode.Ldftn:
                    {
                        string methodFullName = (string)instruction.Operand;
                        HotfixFunc hotfixFunc = HotfixRunner.Create(methodFullName.Split(' ')[1]);

                        object target = hotfixFunc.IsStatic ? null : stack.Pop();

                        Delegate func = hotfixFunc.CreateDelegate(target);
                        stack.Push(func);
                        return instruction.NextOffset;
                    }
                default: throw new Exception("未识别的IL指令:" + instruction.Code.ToString());
            }
        }

        private Delegate CreateDelegate(object target)
        {
            Type delegateType = GetDelegateType();
            return Type switch
            {
                MethodType.Base when MethodBase is MethodInfo methodInfo => methodInfo.CreateDelegate(delegateType, target),
                MethodType.Base => throw new NotImplementedException(),
                MethodType.Hotfix => CreateHotFixDelegate(target),
                _ => throw new NotImplementedException(),
            };
        }

        private Delegate CreateHotFixDelegate(object target)
        {
            Type delegateType = GetDelegateType();
            if (ReturnType == typeof(void))
            {
                DelegateDefines delegateDefines = new DelegateDefines(this, target);
                MethodInfo method = typeof(DelegateDefines).GetMethod($"A{ParametersCount}");
                AssertNotNull(method, $"找不到：DelegateDefines.A{ParametersCount}");
                method = method.MakeGenericMethod(Parameters);
                return Delegate.CreateDelegate(delegateType, delegateDefines, method);
            }
            else
            {
                DelegateDefines delegateDefines = new DelegateDefines(this, target);
                MethodInfo method = typeof(DelegateDefines).GetMethod($"F{ParametersCount}");
                AssertNotNull(method, $"找不到：DelegateDefines.F{ParametersCount}");
                method = method.MakeGenericMethod(delegateType.GetGenericArguments());
                return Delegate.CreateDelegate(delegateType, delegateDefines, method);
            }
        }

        private Type GetDelegateType()
        {
            Type delegateType;
            if (ReturnType == typeof(void))
            {
                delegateType = ParametersCount switch
                {
                    0 => typeof(Action),
                    1 => typeof(Action<>),
                    2 => typeof(Action<,>),
                    3 => typeof(Action<,,>),
                    4 => typeof(Action<,,,>),
                    5 => typeof(Action<,,,,>),
                    6 => typeof(Action<,,,,,>),
                    7 => typeof(Action<,,,,,,>),
                    8 => typeof(Action<,,,,,,,>),
                    9 => typeof(Action<,,,,,,,,>),
                    10 => typeof(Action<,,,,,,,,,>),
                    11 => typeof(Action<,,,,,,,,,,>),
                    12 => typeof(Action<,,,,,,,,,,,>),
                    13 => typeof(Action<,,,,,,,,,,,,>),
                    14 => typeof(Action<,,,,,,,,,,,,,>),
                    15 => typeof(Action<,,,,,,,,,,,,,,>),
                    16 => typeof(Action<,,,,,,,,,,,,,,,>),
                    _ => throw new NotImplementedException(),
                };
                delegateType = delegateType.MakeGenericType(Parameters);
            }
            else
            {
                delegateType = ParametersCount switch
                {
                    0 => typeof(Func<>),
                    1 => typeof(Func<,>),
                    2 => typeof(Func<,,>),
                    3 => typeof(Func<,,,>),
                    4 => typeof(Func<,,,,>),
                    5 => typeof(Func<,,,,,>),
                    6 => typeof(Func<,,,,,,>),
                    7 => typeof(Func<,,,,,,,>),
                    8 => typeof(Func<,,,,,,,,>),
                    9 => typeof(Func<,,,,,,,,,>),
                    10 => typeof(Func<,,,,,,,,,,>),
                    11 => typeof(Func<,,,,,,,,,,,>),
                    12 => typeof(Func<,,,,,,,,,,,,>),
                    13 => typeof(Func<,,,,,,,,,,,,,>),
                    14 => typeof(Func<,,,,,,,,,,,,,,>),
                    15 => typeof(Func<,,,,,,,,,,,,,,,>),
                    16 => typeof(Func<,,,,,,,,,,,,,,,,>),
                    _ => throw new NotImplementedException(),
                };
                Type[] paramTypes = Parameters
                    .Append(ReturnType)
                    .ToArray();
                delegateType = delegateType.MakeGenericType(paramTypes);
            }
            return delegateType;
        }

        private static void AssertNotNull(object obj, string message)
        {
            if (obj == null)
                throw new ArgumentNullException(message);
        }
    }

    public class DelegateDefines
    {
        private readonly HotfixFunc _hotfixFunc;
        private readonly object _target;

        public DelegateDefines(HotfixFunc hotfixFunc, object target)
        {
            _hotfixFunc = hotfixFunc;
            _target = target;
        }

        public void A0()
        {
            _hotfixFunc.InvokeVoid(_target);
        }

        public void A1<T1>(T1 v1)
        {
            _hotfixFunc.InvokeVoid(_target, v1);
        }

        public void A2<T1, T2>(T1 v1, T1 v2)
        {
            _hotfixFunc.InvokeVoid(_target, v1, v2);
        }

        public TResult F0<TResult>()
        {
            return (TResult)_hotfixFunc.Invoke(_target);
        }

        public TResult F1<T1, TResult>(T1 v1)
        {
            return (TResult)_hotfixFunc.Invoke(_target, v1);
        }

        public TResult F2<T1, T2, TResult>(T1 v1, T2 v2)
        {
            return (TResult)_hotfixFunc.Invoke(_target, v1, v2);
        }
    }
}
