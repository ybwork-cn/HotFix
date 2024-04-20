using System.Collections.Generic;

namespace Hotfix
{
    public class HotfixMethodBodyInfo
    {
        public int MaxStackSize;
        public string[] Variables;
        public Dictionary<int, HotfixInstruction> Instructions;

        public HotfixMethodBodyInfo(int maxStackSize, string[] variables, Dictionary<int, HotfixInstruction> instructions)
        {
            MaxStackSize = maxStackSize;
            Variables = variables;
            Instructions = instructions;
        }
    }
}
