using Hotfix.Editor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace HotFix
{
    public class ScriptCompilationCallback : IPostBuildPlayerScriptDLLs
    {
        public int callbackOrder => 0;

        /// <summary>
        /// 在每次修改代码并保存后执行回调
        /// </summary>
        [DidReloadScripts]
        public static void OnScriptsReloaded()
        {
            //CompilationPipeline.compilationFinished += CompilationFinishedBuild;
        }

        /// <summary>
        /// 在每次编译完成后执行回调
        /// </summary>
        //private static void CompilationFinishedBuild(object obj)
        //{
        //    CompilationPipeline.compilationFinished -= CompilationFinishedBuild;

        //    //代码注入
        //    Debug.Log("注入Dll");
        //    //ScriptInjection.Main(@"Library\ScriptAssemblies\Assembly-CSharp.dll");
        //}

        /// <summary>
        /// 每次发布时，编译成功，但未输出到目标文件夹时触发
        /// ---- 如果是IL2CPP模式，则在此函数触发后才进行代码转换
        /// </summary>
        /// <param name="report"></param>
        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            Debug.Log("处理Dll");

            // 代码注入
            string dllPath = @"Temp/StagingArea/Data/Managed/Hotfix.dll";
            ScriptInjection.ILInjection(dllPath);
        }
    }
}
