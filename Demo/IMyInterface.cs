using System.Globalization;

namespace Demo
{
    internal interface IMyInterface
    {
        [System.ComponentModel.DefaultValue(69)]
        int Property1 { get; set; }

        bool Property2 { get; set; }

        [System.ComponentModel.DefaultValue(true)]
        bool Property3 { get; set; }

        [System.ComponentModel.DefaultValue("Hello Roslyn!")]
        string Property4 { get; set; }

        CultureInfo Property5 { get; set; }

        string? NullableString { get; set; }

        [System.ComponentModel.DefaultValue("Default value")]
        string? NullableStringWithDefault { get; set; }

        [System.ComponentModel.DefaultValue(float.NaN)]
        float FloatNaN { get; set; }

        [System.ComponentModel.DefaultValue(float.PositiveInfinity)]
        float FloatPositiveInfinity { get; set; }

        [System.ComponentModel.DefaultValue(float.NegativeInfinity)]
        float FloatNegativeInfinity { get; set; }

        [System.ComponentModel.DefaultValue(float.MinValue)]
        float FloatMinValue { get; set; }

        [System.ComponentModel.DefaultValue(float.MaxValue)]
        float FloatMaxValue { get; set; }

        [System.ComponentModel.DefaultValue(float.Epsilon)]
        float FloatEpsilon { get; set; }

        [System.ComponentModel.DefaultValue(double.NaN)]
        double DoubleNaN { get; set; }

        [System.ComponentModel.DefaultValue(double.PositiveInfinity)]
        double DoublePositiveInfinity { get; set; }

        [System.ComponentModel.DefaultValue(double.NegativeInfinity)]
        double DoubleNegativeInfinity { get; set; }

        [System.ComponentModel.DefaultValue(double.MinValue)]
        double DoubleMinValue { get; set; }

        [System.ComponentModel.DefaultValue(double.MaxValue)]
        double DoubleMaxValue { get; set; }

        [System.ComponentModel.DefaultValue(double.Epsilon)]
        double DoubleEpsilon { get; set; }

        [System.ComponentModel.DefaultValue(long.MinValue)]
        long LongMinValue { get; set; }

        [System.ComponentModel.DefaultValue(long.MaxValue)]
        long LongMaxValue { get; set; }

        [System.ComponentModel.DefaultValue(ulong.MaxValue)]
        ulong ULongMaxValue { get; set; }

        [System.ComponentModel.DefaultValue(DayOfWeek.Friday)]
        DayOfWeek EnumDefault { get; set; }
    }
}
