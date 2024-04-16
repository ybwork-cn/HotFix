using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;
using System.Linq;
using UnityEngine;

namespace HotFix
{
    public static class ScriptInjection
    {
        public static void Generate(string dllPath)
        {
            FileStream fs = new(dllPath, FileMode.Open);
            AssemblyDefinition assembiy = AssemblyDefinition.ReadAssembly(fs);

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

            // 保存并释放dll文件占用
            assembiy.Write(fs);
            fs.Dispose();
        }

        private static void GenerateIL(MethodDefinition methodDefinition)
        {
            Debug.Log("----Parameters");
            foreach (ParameterDefinition item in methodDefinition.Parameters)
            {
                Debug.Log(item.ParameterType.FullName);
            }

            MethodBody body = methodDefinition.Body;

            Debug.Log("----Variables");
            foreach (VariableDefinition item in body.Variables)
            {
                Debug.Log(item.VariableType.FullName);
            }

            Debug.Log("----Instructions");
            foreach (Instruction item in body.Instructions)
            {
                if (item.Operand != null)
                    Debug.Log(item.OpCode + "--" + item.Operand.GetType() + "--" + item.Operand);
                else
                    Debug.Log(item.OpCode);
            }
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
