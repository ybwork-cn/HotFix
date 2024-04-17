namespace Hotfix
{
    public enum OperandType
    {
        String,
        Method,
        Type,
        Instruction,
    }

    public class HotfixInstruction
    {
        public int Offset;
        public HotfixOpCode Code;
        public OperandType OperandType;
        public object Operand;
    }
}
