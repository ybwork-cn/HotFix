using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class HotFixAttribute : Attribute
{
    public readonly string Name;

    public HotFixAttribute() { }

    public HotFixAttribute(string name)
    {
        Name = name;
    }
}

public class Main : MonoBehaviour
{
    void Start()
    {
        Debug.Log(Add(1, 2));
    }

    [HotFix]
    public int Add(int a, int b)
    {
        return a + b;
    }
}
