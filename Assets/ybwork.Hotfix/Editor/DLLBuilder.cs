using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Hotfix.Editor
{
    internal static class DLLBuilder
    {
        private const string DLL_NAME = "Library/ScriptAssemblies/Hotfix.dll";

        [MenuItem("Tools/Switch to Release Mode", priority = 0)]
        public static void SwitchToReleaseMode()
        {
            CompilationPipeline.codeOptimization = CodeOptimization.Release;
        }

        [MenuItem("Tools/Build DLL", priority = 1)]
        public static void BuildDLL()
        {
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
        }

        [MenuItem("Tools/Generate Hotfix IL", priority = 2)]
        public static void GenerateHotfixIL()
        {
            if (Directory.Exists(HotfixRunner.RootPath))
                Directory.Delete(HotfixRunner.RootPath, true);
            Directory.CreateDirectory(HotfixRunner.RootPath);

            List<string> methodNames = new List<string>();
            IEnumerable<HotfixMethodInfo> methods = ScriptInjection.GenerateHotfixIL(DLL_NAME);
            foreach (HotfixMethodInfo method in methods)
            {
                string content = JsonConvert.SerializeObject(method, Formatting.Indented);
                string name = method.Name;
                methodNames.Add(name);
                string path = Path.Combine(HotfixRunner.RootPath, name + ".json");
                File.WriteAllText(path, content);
            }
            string catalogueContent = JsonConvert.SerializeObject(methodNames, Formatting.Indented);
            File.WriteAllText(Path.Combine(HotfixRunner.RootPath, "catalogue.json"), catalogueContent);

            Debug.Log("Generate Hotfix IL Succeed: " + HotfixRunner.RootPath);
            //System.Diagnostics.Process.Start(HotfixRunner.RootPath);
        }

        [MenuItem("Tools/IL Injection", priority = 3)]
        public static void ILInjection()
        {
            ScriptInjection.ILInjection("Library/ScriptAssemblies/Hotfix.dll");
            Debug.Log("IL Injection Succeed");
        }
    }
}
