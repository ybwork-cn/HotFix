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
    [SerializeField] public string aaa;
    [SerializeField]
    public string bbb
    {
        get;
        [HotFix]
        private set;
    }
    void Start()
    {
        aaa = "456";
        Debug.Log(aaa);
        Debug.Log(Add(1, 2));
    }

    [HotFix]
    public int Add(int a, int b)
    {
        return a + b;
    }
}
