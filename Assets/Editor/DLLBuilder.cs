using UnityEditor;
using UnityEditor.Compilation;

public static class DLLBuilder
{
    [MenuItem("Tools/Build DLL")]
    public static void BuildDLL()
    {
        CompilationPipeline.codeOptimization = CodeOptimization.Release;
        CompilationPipeline.RequestScriptCompilation();
    }
}
