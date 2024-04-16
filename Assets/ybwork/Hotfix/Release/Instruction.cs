namespace Hotfix
{
    public enum OperandType
    {
        Method,
        Type,
        Instruction,
    }

    public class Instruction
    {
        public int Offset;
        public HotfixOpCode Code;
        public OperandType OperandType;
        public object Operand;
    }
}
