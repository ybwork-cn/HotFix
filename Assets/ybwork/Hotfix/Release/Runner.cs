using Newtonsoft.Json;

namespace Hotfix
{
    public class Runner
    {
        public static HotfixMethodInfo Create(string content)
        {
            HotfixMethodInfo method = JsonConvert.DeserializeObject<HotfixMethodInfo>(content);
            return method;
        }
    }
}
