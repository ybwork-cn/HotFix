using Hotfix;
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
        Debug.Log(Add(1, 2));
    }

    [Hotfix]
    public int Add(int a, int b)
    {
        int v = a - b;
        return v;
    }
}
