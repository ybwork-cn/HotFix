using System;

namespace Hotfix
{
    public enum OpValueType
    {
        Int = 0,
        Long,
        Sbyte,
        Float,
        Double,
        Bool,
        String,
        Object,
    }

    public readonly struct OpValue
    {
        public readonly OpValueType ValueType;
        public readonly sbyte SbyteValue;
        public readonly int IntValue;
        public readonly long LongValue;
        public readonly float FloatValue;
        public readonly double DoubleValue;
        public readonly bool BoolValue;
        public readonly string StringValue;

        public readonly object ObjectValue;
        public readonly Type ObjectType;

        public readonly object RealValue => ValueType switch
        {
            OpValueType.Int => IntValue,
            OpValueType.Long => LongValue,
            OpValueType.Sbyte => SbyteValue,
            OpValueType.Float => FloatValue,
            OpValueType.Double => DoubleValue,
            OpValueType.Bool => BoolValue,
            OpValueType.String => StringValue,
            OpValueType.Object => ObjectValue,
            _ => throw new NotImplementedException(),
        };

        public OpValue(object value) : this()
        {
            if (value != null)
                ObjectType = value.GetType();
            else
                ObjectType = typeof(object);

            if (value is int intValue)
            {
                ValueType = OpValueType.Int;
                IntValue = intValue;
            }
            else if (value is long longValue)
            {
                ValueType = OpValueType.Long;
                LongValue = longValue;
            }
            else if (value is sbyte sbyteValue)
            {
                ValueType = OpValueType.Sbyte;
                SbyteValue = sbyteValue;
            }
            else if (value is float floatValue)
            {
                ValueType = OpValueType.Float;
                FloatValue = floatValue;
            }
            else if (value is double doubleValue)
            {
                ValueType = OpValueType.Bool;
                DoubleValue = doubleValue;
            }
            else if (value is bool boolValue)
            {
                ValueType = OpValueType.Bool;
                BoolValue = boolValue;
            }
            else if (value is string stringValue)
            {
                ValueType = OpValueType.String;
                StringValue = stringValue;
            }
            else
            {
                ValueType = OpValueType.Object;
            }
            ObjectValue = value;
        }

        public override string ToString()
        {
            return $"[{ObjectType}]{RealValue}";
        }
    }
}
