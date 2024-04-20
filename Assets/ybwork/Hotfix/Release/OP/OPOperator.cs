using System;

namespace Hotfix
{
    public static class OPOperator
    {
        private static Exception GetException(string operateString, OpValue v1, OpValue v2)
        {
            string str = $"'{operateString}'操作符不允许出现在{v1.ObjectType}与{v2.ObjectType}之间";
            return new Exception(str);
        }

        public static OpValue Add(OpValue v1, OpValue v2)
        {
            if (v1.ValueType == OpValueType.String || v2.ValueType == OpValueType.String)
            {
                return new OpValue(v1.RealValue.ToString() + v2.RealValue.ToString());
            }

            if (v1.ValueType == OpValueType.Float && v2.ValueType == OpValueType.Float)
            {
                return new OpValue(v1.FloatValue + v2.FloatValue);
            }

            if (v1.ValueType == OpValueType.Float)
            {
                if (v2.ValueType == OpValueType.Int)
                {
                    return new OpValue(v1.FloatValue + v2.IntValue);
                }
                throw GetException("+", v1, v2);
            }
            if (v2.ValueType == OpValueType.Float)
            {
                if (v1.ValueType == OpValueType.Int)
                {
                    return new OpValue(v1.IntValue + v2.FloatValue);
                }
                throw GetException("+", v1, v2);
            }

            if (v1.ValueType == OpValueType.Int && v2.ValueType == OpValueType.Int)
                return new OpValue(v1.IntValue + v2.IntValue);
            throw GetException("+", v1, v2);
        }

        public static OpValue Sub(OpValue v1, OpValue v2)
        {
            if (v1.ValueType == OpValueType.Float && v2.ValueType == OpValueType.Float)
                return new OpValue(v1.FloatValue - v2.FloatValue);
            if (v1.ValueType == OpValueType.Float && v2.ValueType == OpValueType.Int)
                return new OpValue(v1.FloatValue - v2.IntValue);
            if (v1.ValueType == OpValueType.Int && v2.ValueType == OpValueType.Float)
                return new OpValue(v1.IntValue - v2.FloatValue);
            if (v1.ValueType == OpValueType.Int && v2.ValueType == OpValueType.Int)
                return new OpValue(v1.IntValue - v2.IntValue);

            throw GetException("-", v1, v2);
        }

        public static OpValue Mul(OpValue v1, OpValue v2)
        {
            if (v1.ValueType == OpValueType.Float && v2.ValueType == OpValueType.Float)
                return new OpValue(v1.FloatValue * v2.FloatValue);
            if (v1.ValueType == OpValueType.Float && v2.ValueType == OpValueType.Int)
                return new OpValue(v1.FloatValue * v2.IntValue);
            if (v1.ValueType == OpValueType.Int && v2.ValueType == OpValueType.Float)
                return new OpValue(v1.IntValue * v2.FloatValue);
            if (v1.ValueType == OpValueType.Int && v2.ValueType == OpValueType.Int)
                return new OpValue(v1.IntValue * v2.IntValue);

            throw GetException("*", v1, v2);
        }

        public static OpValue Div(OpValue v1, OpValue v2)
        {
            if (v1.ValueType == OpValueType.Float && v2.ValueType == OpValueType.Float)
                return new OpValue(v1.FloatValue / v2.FloatValue);
            if (v1.ValueType == OpValueType.Float && v2.ValueType == OpValueType.Int)
                return new OpValue(v1.FloatValue / v2.IntValue);
            if (v1.ValueType == OpValueType.Int && v2.ValueType == OpValueType.Float)
                return new OpValue(v1.IntValue / v2.FloatValue);
            if (v1.ValueType == OpValueType.Int && v2.ValueType == OpValueType.Int)
                return new OpValue(v1.IntValue / v2.IntValue);

            throw GetException("/", v1, v2);
        }

        public static OpValue Less(OpValue v1, OpValue v2)
        {
            var v1_new = ChangeType_Compare(v1);
            var v2_new = ChangeType_Compare(v2);

            if (v1_new.ValueType == OpValueType.Int && v2_new.ValueType == OpValueType.Int)
                return new OpValue(v1_new.IntValue < v2_new.IntValue);
            if (v1_new.ValueType == OpValueType.Double && v2_new.ValueType == OpValueType.Double)
                return new OpValue(v1_new.DoubleValue < v2_new.DoubleValue);

            throw GetException("<", v1, v2);
        }

        public static OpValue Greater(OpValue v1, OpValue v2)
        {
            var v1_new = ChangeType_Compare(v1);
            var v2_new = ChangeType_Compare(v2);

            if (v1_new.ValueType == OpValueType.Int && v2_new.ValueType == OpValueType.Int)
                return new OpValue(v1_new.IntValue > v2_new.IntValue);
            if (v1_new.ValueType == OpValueType.Double && v2_new.ValueType == OpValueType.Double)
                return new OpValue(v1_new.DoubleValue > v2_new.DoubleValue);

            throw GetException(">", v1, v2);
        }

        public static OpValue Equal(OpValue v1, OpValue v2)
        {
            var v1_new = ChangeType_Compare(v1);
            var v2_new = ChangeType_Compare(v2);

            if (v1_new.ValueType == OpValueType.Int && v2_new.ValueType == OpValueType.Int)
                return new OpValue(v1_new.IntValue == v2_new.IntValue);
            if (v1_new.ValueType == OpValueType.Double && v2_new.ValueType == OpValueType.Double)
                return new OpValue(v1_new.DoubleValue == v2_new.DoubleValue);

            throw GetException("==", v1, v2);
        }

        private static OpValue ChangeType_Compare(OpValue v)
        {
            if (v.ValueType == OpValueType.Sbyte)
                v = new OpValue((int)v.SbyteValue);
            if (v.ValueType == OpValueType.Float)
                v = new OpValue((double)v.FloatValue);
            return v;
        }
    }
}
