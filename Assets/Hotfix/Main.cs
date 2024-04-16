using Hotfix;
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
        Debug.Log(Add(1, 2));
        string content = File.ReadAllText(Application.streamingAssetsPath + "/aa.json");
        HotfixMethodInfo method = Runner.Create(content);
    }

    [Hotfix]
    public int Add(int a, int b)
    {
        int v = a + b;
        Debug.Log(v);
        return v;
    }
}
