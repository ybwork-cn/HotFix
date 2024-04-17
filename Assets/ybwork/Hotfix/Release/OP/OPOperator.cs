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
            if (v1.ValueType == OpValueType.Double)
            {
                v1 = new OpValue((float)v1.DoubleValue);
            }
            if (v2.ValueType == OpValueType.Double)
            {
                v2 = new OpValue((float)v2.DoubleValue);
            }

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
            if (v1.ValueType == OpValueType.Double)
            {
                v1 = new OpValue((float)v1.DoubleValue);
            }
            if (v2.ValueType == OpValueType.Double)
            {
                v2 = new OpValue((float)v2.DoubleValue);
            }

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
            if (v1.ValueType == OpValueType.Double)
            {
                v1 = new OpValue((float)v1.DoubleValue);
            }
            if (v2.ValueType == OpValueType.Double)
            {
                v2 = new OpValue((float)v2.DoubleValue);
            }

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
            if (v1.ValueType == OpValueType.Double)
            {
                v1 = new OpValue((float)v1.DoubleValue);
            }
            if (v2.ValueType == OpValueType.Double)
            {
                v2 = new OpValue((float)v2.DoubleValue);
            }

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
            if (v1.ValueType == OpValueType.Double)
            {
                v1 = new OpValue((float)v1.DoubleValue);
            }
            if (v2.ValueType == OpValueType.Double)
            {
                v2 = new OpValue((float)v2.DoubleValue);
            }

            if (v1.ValueType == OpValueType.Int)
            {
                v1 = new OpValue((float)v1.IntValue);
            }
            if (v2.ValueType == OpValueType.Int)
            {
                v2 = new OpValue((float)v2.IntValue);
            }

            if (v1.ValueType == OpValueType.Float && v1.ValueType == OpValueType.Float)
                return new OpValue(v1.FloatValue < v2.FloatValue);

            throw GetException("<", v1, v2);
        }

        public static OpValue Greater(OpValue v1, OpValue v2)
        {
            if (v1.ValueType == OpValueType.Double)
            {
                v1 = new OpValue((float)v1.DoubleValue);
            }
            if (v2.ValueType == OpValueType.Double)
            {
                v2 = new OpValue((float)v2.DoubleValue);
            }

            if (v1.ValueType == OpValueType.Int)
            {
                v1 = new OpValue((float)v1.IntValue);
            }
            if (v2.ValueType == OpValueType.Int)
            {
                v2 = new OpValue((float)v2.IntValue);
            }

            if (v1.ValueType == OpValueType.Float && v1.ValueType == OpValueType.Float)
                return new OpValue(v1.FloatValue > v2.FloatValue);

            throw GetException(">", v1, v2);
        }
    }
}
