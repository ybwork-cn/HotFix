﻿using Hotfix;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        int result = arr.Sum();
        Log(result + "---");
        return result;
    }

    [Hotfix]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Log(string msg)
    {
        Debug.Log(msg);
    }
}
