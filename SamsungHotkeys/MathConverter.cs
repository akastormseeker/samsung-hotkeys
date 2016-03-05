using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SamsungHotkeys
{
    class MathConverter : IValueConverter
    {
        public Operator Operator { get; set; }
        public double OtherValue { get; set; }
        public Side BoundSide { get; set; } 
        
        public MathConverter()
        {
            BoundSide = Side.Left;
            OtherValue = 1.0;
            Operator = Operator.Add;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double lhs = (BoundSide == Side.Left) ? (double)value : OtherValue;
            double rhs = (BoundSide == Side.Left) ? OtherValue : (double)value;
            switch(Operator)
            {
                case Operator.Add: return lhs + rhs;
                case Operator.Subtract: return lhs - rhs;
                case Operator.Multiply: return lhs * rhs;
                case Operator.Divide: return lhs / rhs;
                case Operator.Power: return Math.Pow(lhs, rhs);
                case Operator.Max: return Math.Max(lhs, rhs);
                case Operator.Min: return Math.Min(lhs, rhs);
                case Operator.GreaterThan: return (lhs > rhs) ? 1.0 : 0.0;
                case Operator.LessThan: return (lhs < rhs) ? 1.0 : 0.0;
                case Operator.GreaterThanOrEqual: return (lhs >= rhs) ? 1.0 : 0.0;
                case Operator.LessThanOrEqual: return (lhs <= rhs) ? 1.0 : 0.0;
                case Operator.Equal: return (lhs == rhs) ? 1.0 : 0.0;
                case Operator.Sin: return Math.Sin(lhs);
                case Operator.Cos: return Math.Cos(lhs);
                case Operator.Tan: return Math.Tan(lhs);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum Side
    {
        Left,
        Right
    }

    public enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Power,
        Max,
        Min,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Equal,
        Sin,
        Cos,
        Tan
    }
}
