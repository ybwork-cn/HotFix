using Hotfix;
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
        Add(new List<int> { 3, 4, 5, 6 });
    }

    public async void Add(IEnumerable<int> arr)
    {
        int result = arr.Sum();
        var x = await Delay(300);
        Debug.Log(x + ":" + result);

    }

    public async YueTask<int> Delay(int ms)
    {
        await YueTask.Delay(0.6f);
        Debug.Log(ms);
        return ms;
    }
}
