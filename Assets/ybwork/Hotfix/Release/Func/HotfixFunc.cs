using System;
using System.Collections;

namespace Hotfix
{
    public class HotfixFunc
    {
        internal readonly HotfixMethodInfo MethodInfo;

        internal HotfixFunc(HotfixMethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
        }

        public object Invoke(params object[] paras)
        {
            object[] vars = new object[MethodInfo.Body.Variables.Length];
            Stack stack = new Stack();
            int instructionIndex = 0;
            while (true)
            {
                HotfixInstruction instruction = MethodInfo.Body.Instructions[instructionIndex];
                switch (instruction.Code)
                {
                    case HotfixOpCode.Ldarg_1:
                        stack.Push(paras[0]);
                        instructionIndex++;
                        break;
                    case HotfixOpCode.Ldarg_2:
                        stack.Push(paras[1]);
                        instructionIndex++;
                        break;
                    case HotfixOpCode.Add:
                        {
                            object a = stack.Pop();
                            object b = stack.Pop();
                            var result = OPOperator.Add(new OpValue(a), new OpValue(b)).ObjectValue;
                            stack.Push(result);
                            instructionIndex++;
                        }
                        break;
                    case HotfixOpCode.Ret:
                        {
                            object result = stack.Pop();
                            return result;
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
