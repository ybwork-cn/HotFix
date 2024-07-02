using Hotfix;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ybwork.Async;

public class Main : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return HotfixRunner.InitAsync();
        Debug.Log("初始化完成");
        new Test().Add(new List<int> { 3, 4, 5, 6 });
    }
}

public class Test
{
    public void Add(IEnumerable<int> arr)
    {

        var names = typeof(Main).Assembly.GetTypes().Select(type => type.FullName);
        Debug.Log(JsonConvert.SerializeObject(names, Formatting.Indented));

        int result = arr.Sum();
        int v = result / 3;
        switch (v)
        {
            case 4:
            case 6:
            case 8:
            case 20:
                Debug.Log(v + ":" + result);
                break;
        }
        Delay(100).Then(v => Debug.Log(v));
    }

    public async YueTask<int> Delay(int ms)
    {
        await YueTask.Delay(0.6f);
        Debug.Log(ms);
        return ms;
    }
}
