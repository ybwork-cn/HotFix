﻿using Hotfix;
using System.IO;
using UnityEngine;

public class Main : MonoBehaviour
{
    public string aaa;
    public string bbb
    {
        get;
        //[Hotfix]
        private set;
    }

    void Start()
    {
        aaa = "456";
        Debug.Log(aaa);
        //Debug.Log(Add(1, 2));
        string content = File.ReadAllText(Application.streamingAssetsPath + "/aa.json");
        HotfixFunc method = HotfixRunner.Create(content);
        var result = method.Invoke(1, 2);
        Debug.Log(result);
    }

    [Hotfix]
    public int Add(int a, int b)
    {
        int v = a + b;
        return v;
    }
}
