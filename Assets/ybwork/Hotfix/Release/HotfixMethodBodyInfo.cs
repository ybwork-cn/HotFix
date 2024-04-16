namespace Hotfix
{
    public class HotfixMethodBodyInfo
    {
        public int MaxStackSize;
        public string[] Variables;
        public Instruction[] Instructions;

        public HotfixMethodBodyInfo(int maxStackSize, string[] variables, Instruction[] instructions)
        {
            MaxStackSize = maxStackSize;
            Variables = variables;
            Instructions = instructions;
        }
    }
}
