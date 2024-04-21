using Hotfix;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Main : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return HotfixRunner.InitAsync();
        Debug.Log("初始化完成");
        Debug.Log("返回值:" + Add(new List<int> { 3, 4, 5, 6 }));
    }

    [Hotfix]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Add(IEnumerable<int> arr)
    {
        int result = 0;
        foreach (var item in arr)
        {
            if ((item >> 1) == 3)
                continue;
            result += item;
            if ((item << 1) == 4)
                break;
        }
        return result;
    }
}
