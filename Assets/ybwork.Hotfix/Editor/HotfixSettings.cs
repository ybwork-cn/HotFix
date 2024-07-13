using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CreateAssetMenu(menuName = "HotFix/HotfixSettings", fileName = "HotfixSettings")]
public class HotfixSettings : ScriptableObject
{
    [SerializeField] AssemblyDefinitionAsset[] _hotfixAssemblies;

    public static string[] GetAllDllNames()
    {
        string settingsGUID = AssetDatabase.FindAssets("t:" + nameof(HotfixSettings)).FirstOrDefault();
        if (settingsGUID == null)
            return Array.Empty<string>();
        else
        {
            string settingsPath = AssetDatabase.GUIDToAssetPath(settingsGUID);
            HotfixSettings settings = AssetDatabase.LoadAssetAtPath<HotfixSettings>(settingsPath);
            return settings._hotfixAssemblies
                .Select(assembly => (string)JObject.Parse(assembly.text)["name"])
                .ToArray();
        }
    }
}
