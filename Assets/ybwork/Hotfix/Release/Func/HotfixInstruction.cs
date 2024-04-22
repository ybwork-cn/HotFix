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
        Variable,
        Type,
        Instruction,
    }

    public class HotfixInstruction
    {
        public HotfixOpCode Code;
        public OperandType OperandType;
        public string CodeString;
        public object Operand;
        public int NextOffset;

        public override string ToString()
        {
            if (Operand == null)
                return Code.ToString();
            else
                return Code + " " + Operand;
        }
    }
}
