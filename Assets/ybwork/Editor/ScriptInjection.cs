using Mono.Cecil;
using Mono.Cecil.Cil;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Hotfix.Editor
{
    public static class ScriptInjection
    {
        public static void Generate(string dllPath)
        {
            FileStream fs = new(dllPath, FileMode.Open);
            AssemblyDefinition assembiy = AssemblyDefinition.ReadAssembly(fs);

            try
            {
                /// 遍历程序所有方法，进行代码注入
                /// 对所有方法，嵌入一段代码，检查该方法是否为热更新代码，如果是热更新代码，执行热更新逻辑
                /// 对标记了<see cref="HotfixAttribute"/>的方法，将其IL序列化
                foreach (TypeDefinition typeDef in assembiy.MainModule.Types)
                {
                    foreach (MethodDefinition methedDef in typeDef.Methods)
                    {
                        string hotFixAttribute = typeof(HotfixAttribute).FullName;
                        if (methedDef.CustomAttributes.Any(attr => attr.AttributeType.FullName == hotFixAttribute))
                        {
                            // IL序列化
                            GenerateIL(methedDef);
                        }
                        // 代码注入
                        InjectionHotfix(assembiy, methedDef);
                    }
                }
            }
            finally
            {
                // 保存并释放dll文件占用
                //assembiy.Write(fs);
                fs.Dispose();
            }
        }

        private static void GenerateIL(MethodDefinition methodDefinition)
        {
            HotfixMethodInfo methodInfo = Convert(methodDefinition);
            string content = JsonConvert.SerializeObject(methodInfo, Formatting.Indented);
            Debug.Log(content);
            Directory.CreateDirectory(Application.streamingAssetsPath);
            File.WriteAllText(Application.streamingAssetsPath + "/aa.json", content);
        }

        private static HotfixMethodInfo Convert(MethodDefinition methodDefinition)
        {
            string name = methodDefinition.Name;
            string[] parameters = methodDefinition.Parameters
                .Select(p => p.ParameterType.FullName)
                .ToArray();
            string returnType = methodDefinition.ReturnType.FullName;
            HotfixMethodBodyInfo bodyInfo = Convert(methodDefinition.Body);
            return new HotfixMethodInfo
            {
                Name = name,
                Parameters = parameters,
                ReturnType = returnType,
                Body = bodyInfo,
            };
        }

        private static HotfixMethodBodyInfo Convert(MethodBody methodBody)
        {
            int maxStackSize = methodBody.MaxStackSize;
            string[] variables = methodBody.Variables
                .Select(v => v.VariableType.FullName)
                .ToArray();
            Instruction[] instructions = methodBody.Instructions
                .Select(instruction =>
                {
                    int offset = instruction.Offset;
                    HotfixOpCode code = (HotfixOpCode)(int)instruction.OpCode.Code;
                    object operand = instruction.Operand;
                    Instruction result = new Instruction();
                    result.Offset = offset;
                    result.Code = code;
                    if (operand != null)
                    {
                        if (operand is TypeReference typeReference)
                        {
                            result.OperandType = OperandType.Type;
                            result.Operand = typeReference.FullName;
                        }
                        else if (operand is MethodReference methodReference)
                        {
                            result.OperandType = OperandType.Method;
                            result.Operand = methodReference.FullName;
                        }
                        else if (operand is Mono.Cecil.Cil.Instruction targetInstruction)
                        {
                            result.OperandType = OperandType.Instruction;
                            result.Operand = targetInstruction.Offset;
                        }
                        else
                            throw new System.Exception("错误的OperandType:" + operand.GetType());
                    }
                    return result;
                })
                .ToArray();
            return new HotfixMethodBodyInfo(maxStackSize, variables, instructions);
        }

        private static void InjectionHotfix(AssemblyDefinition assembiy, MethodDefinition methodDefinition)
        {
            // 找到需要AOP的函数
            //ILProcessor processor = methodDefinition.Body.GetILProcessor();

            // 注入新的临时变量
            //TypeReference boolReference = assembiy.MainModule.ImportReference(typeof(bool));
            //processor.Body.Variables.Add(new VariableDefinition(boolReference));
            // 记录临时变量的索引位置
            //int flagIndex = processor.Body.Variables.Count - 1;

            //Instruction firstInstruction = processor.Body.Instructions[0];
            //processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Nop));
            //processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldsfld, fieldDefinition));
            //processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldnull));
            //processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Cgt_Un));
            //processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Stloc, flagIndex));
            //processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldloc, flagIndex));
            //processor.InsertBefore(firstInstruction, processor.Create(
            //    OpCodes.Brfalse_S,
            //    processor.Body.Instructions[6]));
            //processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldsfld, fieldDefinition));
            //processor.InsertBefore(firstInstruction, processor.Create(
            //    OpCodes.Ldstr, 
            //    methodDefinition.DeclaringType.FullName + ":" + methodDefinition.Name));
            //for (int i = 1; i <= methodDefinition.Parameters.Count; i++)
            //{
            //    processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg, i));
            //}
            //processor.InsertBefore(firstInstruction, processor.Create(
            //    OpCodes.Callvirt,
            //    assembiy.MainModule.ImportReference(funcType.GetMethod("Invoke"))));
            //processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ret));
        }
    }
}
