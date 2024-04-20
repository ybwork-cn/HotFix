﻿using Hotfix;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Main : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return HotfixRunner.InitAsync();
        Debug.Log("初始化完成");
        Debug.Log("返回值:" + Add(1, 2));
    }

    [Hotfix]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Add(int a, int b)
    {
        int v = a + b;
        return v;
    }
}
