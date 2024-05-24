using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Player;
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
            BuildTarget buildTarget = BuildTarget.StandaloneWindows64;

            ScriptCompilationSettings compilationSettings = new ScriptCompilationSettings();
            compilationSettings.group = BuildPipeline.GetBuildTargetGroup(buildTarget);
            compilationSettings.target = buildTarget;
            compilationSettings.options = ScriptCompilationOptions.None;

            Directory.CreateDirectory($"HotfixDlls/Latest/");
            PlayerBuildInterface.CompilePlayerScripts(compilationSettings, "HotfixDlls/Latest/");
            EditorUtility.ClearProgressBar();

            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var version = (int)(DateTime.Now - DateTime.Now.Date).TotalSeconds;
            string versionPath = $"HotfixDlls/{date}-{version}";

            Directory.CreateDirectory(versionPath);

            string[] files = Directory.GetFiles("HotfixDlls/Latest/");
            foreach (string s in files)
            {
                string fileName = Path.GetFileName(s);
                string destFile = Path.Combine(versionPath, fileName);
                File.Copy(s, destFile, true);
            }
        }

        [MenuItem("Tools/Generate Hotfix IL", priority = 2)]
        public static void GenerateHotfixIL()
        {
            if (Directory.Exists(HotfixRunner.RootPath))
                Directory.Delete(HotfixRunner.RootPath, true);
            Directory.CreateDirectory(HotfixRunner.RootPath);

            string oldDllName = $"HotfixDlls/Latest/Hotfix.dll";

            Dictionary<string, string> methodNames = new Dictionary<string, string>();
            IEnumerable<HotfixMethodInfo> methods = ScriptInjection.GenerateHotfixIL(DLL_NAME, oldDllName);
            foreach (HotfixMethodInfo method in methods)
            {
                string content = JsonConvert.SerializeObject(method, Formatting.Indented);
                string name = method.Name;
                string hash = Hash128.Compute(name).ToString();
                methodNames.Add(name, hash);
                string path = Path.Combine(HotfixRunner.RootPath, hash + ".json");
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
            ScriptInjection.ILInjection(DLL_NAME);
            Debug.Log("IL Injection Succeed");
        }
    }
}
