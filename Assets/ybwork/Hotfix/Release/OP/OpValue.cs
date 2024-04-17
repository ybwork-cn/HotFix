using System;

namespace Hotfix
{
    public enum OpValueType
    {
        Int = 0,
        Float,
        Double,
        Bool,
        String,
        Object,
    }

    public readonly struct OpValue
    {
        public readonly OpValueType ValueType;
        public readonly int IntValue;
        public readonly float FloatValue;
        public readonly double DoubleValue;
        public readonly bool BoolValue;
        public readonly string StringValue;

        public readonly object ObjectValue;
        public readonly Type ObjectType;

        public readonly object RealValue => ValueType switch
        {
            OpValueType.Int => IntValue,
            OpValueType.Float => FloatValue,
            OpValueType.Double => DoubleValue,
            OpValueType.Bool => BoolValue,
            OpValueType.String => StringValue,
            OpValueType.Object => ObjectValue,
            _ => throw new NotImplementedException(),
        };

        public OpValue(int value) : this()
        {
            ValueType = OpValueType.Int;
            ObjectType = typeof(int);
            IntValue = value;
            ObjectValue = value;
        }

        public OpValue(float value) : this()
        {
            ValueType = OpValueType.Float;
            ObjectType = typeof(float);
            FloatValue = value;
            ObjectValue = value;
        }

        public OpValue(double value) : this()
        {
            ValueType = OpValueType.Double;
            ObjectType = typeof(double);
            DoubleValue = value;
            ObjectValue = value;
        }

        public OpValue(bool value) : this()
        {
            ValueType = OpValueType.Bool;
            ObjectType = typeof(bool);
            BoolValue = value;
            ObjectValue = value;
        }

        public OpValue(string value) : this()
        {
            ValueType = OpValueType.String;
            ObjectType = typeof(string);
            StringValue = value;
            ObjectValue = value;
        }

        public OpValue(object value) : this()
        {
            if (value is int intValue)
            {
                ValueType = OpValueType.Int;
                IntValue = intValue;
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
                if (value != null)
                    ObjectType = value.GetType();
                else
                    ObjectType = typeof(object);
            }
            ObjectValue = value;
        }
    }
}
