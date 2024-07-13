using System.Linq;

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
