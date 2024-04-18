using System.Runtime.CompilerServices;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        Debug.Log("返回值:" + Add(1, 2));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Add(int a, int b)
    {
        int v = a - b;
        return v;
    }
}
