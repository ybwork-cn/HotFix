using Hotfix;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Main : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return HotfixRunner.InitAsync();
        Debug.Log("初始化完成");
        Debug.Log("返回值:" + Add(3, 4, 5, 6));
    }

    [Hotfix]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Add(params int[] arr)
    {
        return arr.Sum();
    }
}
