namespace Hotfix
{
    public enum OperandType
    {
        None = 0,
        Int,
        Float,
        String,
        Method,
        Type,
        Instruction,
    }

    public class HotfixInstruction
    {
        public HotfixOpCode Code;
        public OperandType OperandType;
        public object Operand;
    }
}
