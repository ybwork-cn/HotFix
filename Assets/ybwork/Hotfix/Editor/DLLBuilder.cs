using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Hotfix.Editor
{
    public static class DLLBuilder
    {
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
            Directory.Delete(HotfixRunner.RootPath, true);
            Directory.CreateDirectory(HotfixRunner.RootPath);

            ScriptInjection.GenerateHotfixIL("Library/ScriptAssemblies/Hotfix.dll");

            Debug.Log("Generate Hotfix IL Succeed: " + HotfixRunner.RootPath);
        }

        [MenuItem("Tools/IL Injection", priority = 3)]
        public static void ILInjection()
        {
            ScriptInjection.ILInjection("Library/ScriptAssemblies/Hotfix.dll");
            Debug.Log("IL Injection Succeed");
        }
    }
}
