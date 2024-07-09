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
        new Test().Add(new List<int> { 3, 4, 5, 6 });
    }
}

public class Test
{
    public void Add(IEnumerable<int> arr)
    {
        int result = arr.Sum() + GetRange(3, 5).Sum();
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
        Delay(100).Then(v => Debug.Log(NumberUtils.ToString(v)));
    }

    public IEnumerable<int> GetRange(int min, int max)
    {
        for (int i = min; i < max; i++)
        {
            yield return i;
        }
    }

    public async YueTask<int> Delay(int ms)
    {
        Debug.Log(NumberUtils.ToString(ms));
        await YueTask.Delay(0.6f);
        Debug.Log(NumberUtils.ToString(ms));
        return ms;
    }
}
public static class NumberUtils
{
    private const int COUNT = 10000;
    private static readonly string[] _strs = Enumerable.Range(0, COUNT).Select(i => i.ToString()).ToArray();

    public static string ToString(int value)
    {
        if (value >= 0 && value < COUNT)
            return _strs[value];
        else
            return value.ToString();
    }

    public static string ToString(long value)
    {
        if (value >= 0 && value < COUNT)
            return _strs[value];
        else
            return value.ToString();
    }
}
