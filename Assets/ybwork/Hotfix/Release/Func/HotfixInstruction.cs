namespace Hotfix
{
    public enum OperandType
    {
        None = 0,
        Other,
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
        public string CodeString => Code.ToString();
        public object Operand;
        public int NextOffset;

        public override string ToString()
        {
            if (Operand == null)
                return CodeString;
            else
                return CodeString + " " + Operand;
        }
    }
}
