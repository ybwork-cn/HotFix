namespace Hotfix
{
    public class HotfixMethodBodyInfo
    {
        public int MaxStackSize;
        public string[] Variables;
        public HotfixInstruction[] Instructions;

        public HotfixMethodBodyInfo(int maxStackSize, string[] variables, HotfixInstruction[] instructions)
        {
            MaxStackSize = maxStackSize;
            Variables = variables;
            Instructions = instructions;
        }
    }
}
