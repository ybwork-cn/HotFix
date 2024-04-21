using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

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
                    case HotfixOpCode.Nop:
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldarg_0:
                        stack.Push(paras[0]);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldarg_1:
                        stack.Push(paras[1]);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldarg_2:
                        stack.Push(paras[2]);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldarg_3:
                        stack.Push(paras[2]);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldloc_0:
                        stack.Push(vars[0]);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldloc_1:
                        stack.Push(vars[1]);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldloc_2:
                        stack.Push(vars[2]);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldloc_3:
                        stack.Push(vars[3]);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Stloc_0:
                        vars[0] = stack.Pop();
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Stloc_1:
                        vars[1] = stack.Pop();
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Stloc_2:
                        vars[2] = stack.Pop();
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Stloc_3:
                        vars[3] = stack.Pop();
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldarg_S:
                        {
                            int index = Convert.ToInt32(instruction.Operand);
                            stack.Push(paras[index]);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Ldnull:
                        stack.Push(null);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldc_I4_0:
                        stack.Push(0);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldc_I4_1:
                        stack.Push(1);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldc_I4_2:
                        stack.Push(2);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldc_I4_3:
                        stack.Push(3);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldc_I4_4:
                        stack.Push(4);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldc_I4_5:
                        stack.Push(5);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldc_I4_6:
                        stack.Push(6);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldc_I4_7:
                        stack.Push(7);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldc_I4_8:
                        stack.Push(8);
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldc_I4_S:
                        {
                            sbyte value = Convert.ToSByte(instruction.Operand);
                            stack.Push(value);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Dup:
                        {
                            object result = stack.Peek();
                            stack.Push(result);
                            instructionIndex = instruction.NextOffset;
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
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Ret:
                        {
                            object result = stack.Pop();
                            return result;
                        }
                    case HotfixOpCode.Br_S:
                        instructionIndex = Convert.ToInt32(instruction.Operand);
                        break;
                    case HotfixOpCode.Brtrue_S:
                        {
                            object value = stack.Pop();
                            bool boolValue = Convert.ToBoolean(value);
                            if (boolValue)
                                instructionIndex = Convert.ToInt32(instruction.Operand);
                            else
                                instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Beq_S:
                        {
                            object v2 = stack.Pop();
                            object v1 = stack.Pop();
                            // v1==v2
                            var result = OPOperator.Equal(new OpValue(v1), new OpValue(v2)).BoolValue;
                            if (result)
                                instructionIndex = Convert.ToInt32(instruction.Operand);
                            else
                                instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Bge_S:
                        {
                            object v2 = stack.Pop();
                            object v1 = stack.Pop();
                            // v1>=v2
                            var result = OPOperator.Less(new OpValue(v2), new OpValue(v1)).BoolValue;
                            if (result)
                                instructionIndex = Convert.ToInt32(instruction.Operand);
                            else
                                instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Bgt_S:
                        {
                            object v2 = stack.Pop();
                            object v1 = stack.Pop();
                            // v1>v2
                            var result = OPOperator.Greater(new OpValue(v1), new OpValue(v2)).BoolValue;
                            if (result)
                                instructionIndex = Convert.ToInt32(instruction.Operand);
                            else
                                instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Ble_S:
                        {
                            object v2 = stack.Pop();
                            object v1 = stack.Pop();
                            // v1<=v2
                            var result = OPOperator.Greater(new OpValue(v2), new OpValue(v1)).BoolValue;
                            if (result)
                                instructionIndex = Convert.ToInt32(instruction.Operand);
                            else
                                instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Blt_S:
                        {
                            object v2 = stack.Pop();
                            object v1 = stack.Pop();
                            // v1<v2
                            var result = OPOperator.Less(new OpValue(v1), new OpValue(v2)).BoolValue;
                            if (result)
                                instructionIndex = Convert.ToInt32(instruction.Operand);
                            else
                                instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Bne_Un_S:
                        {
                            object v2 = stack.Pop();
                            object v1 = stack.Pop();
                            var result = !OPOperator.Equal(new OpValue(v1), new OpValue(v2)).BoolValue;
                            if (result)
                                instructionIndex = Convert.ToInt32(instruction.Operand);
                            else
                                instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Add:
                        {
                            object v2 = stack.Pop();
                            object v1 = stack.Pop();
                            var result = OPOperator.Add(new OpValue(v2), new OpValue(v1)).ObjectValue;
                            stack.Push(result);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Sub:
                        {
                            object v2 = stack.Pop();
                            object v1 = stack.Pop();
                            var result = OPOperator.Sub(new OpValue(v1), new OpValue(v2)).ObjectValue;
                            stack.Push(result);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Mul:
                        {
                            object v2 = stack.Pop();
                            object v1 = stack.Pop();
                            var result = OPOperator.Mul(new OpValue(v1), new OpValue(v2)).ObjectValue;
                            stack.Push(result);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Div:
                        {
                            object v2 = stack.Pop();
                            object v1 = stack.Pop();
                            object result = OPOperator.Div(new OpValue(v1), new OpValue(v2)).ObjectValue;
                            stack.Push(result);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.And:
                        {
                            int v2 = Convert.ToInt32(stack.Pop());
                            int v1 = Convert.ToInt32(stack.Pop());
                            int result = v1 & v2;
                            stack.Push(result);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Or:
                        {
                            int v2 = Convert.ToInt32(stack.Pop());
                            int v1 = Convert.ToInt32(stack.Pop());
                            int result = v1 | v2;
                            stack.Push(result);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Xor:
                        {
                            int v2 = Convert.ToInt32(stack.Pop());
                            int v1 = Convert.ToInt32(stack.Pop());
                            int result = v1 ^ v2;
                            stack.Push(result);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Shl:
                        {
                            int v2 = Convert.ToInt32(stack.Pop());
                            int v1 = Convert.ToInt32(stack.Pop());
                            int result = v1 << v2;
                            stack.Push(result);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Shr:
                        {
                            int v2 = Convert.ToInt32(stack.Pop());
                            int v1 = Convert.ToInt32(stack.Pop());
                            int result = v1 >> v2;
                            stack.Push(result);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Conv_I4:
                        {
                            object value = stack.Pop();
                            int intValue = Convert.ToInt32(value);
                            stack.Push(intValue);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Callvirt:
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
                            object obj = stack.Pop();
                            if (method.ReturnType == typeof(void))
                                method.Invoke(obj, paraValues);
                            else
                            {
                                object result = method.Invoke(obj, paraValues);
                                stack.Push(result);
                            }
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Box:
                        // TODO:stack中不保存object，而是OpValue，以应对装箱拆箱操作
                        // 装箱指令不需要单独操作
                        instructionIndex = instruction.NextOffset;
                        break;
                    case HotfixOpCode.Ldlen:
                        {
                            object arr = stack.Pop();
                            int len = (arr as Array).Length;
                            stack.Push(len);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Ldelem_I4:
                        {
                            int index = Convert.ToInt32(stack.Pop());
                            Array arr = stack.Pop() as Array;
                            object item = arr.GetValue(index);
                            stack.Push(item);
                            instructionIndex = instruction.NextOffset;
                            break;
                        }
                    case HotfixOpCode.Leave_S:
                        {
                            instructionIndex = Convert.ToInt32(instruction.Operand);
                            break;
                        }
                    default: throw new Exception("未识别的IL指令:" + instruction.Code.ToString());
                }
            }
        }

        public void InvokeVoid(params object[] paras)
        {
            return;
        }
    }
}
