using UnityEditorInternal;
using UnityEngine;

[CreateAssetMenu(menuName = "HotFix/HotfixSettings", fileName = "HotfixSettings")]
public class HotfixSettings : ScriptableObject
{
    public AssemblyDefinitionAsset[] HotfixAssemblies;
}
