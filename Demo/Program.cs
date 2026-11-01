using Demo;

public class Program
{
    public static void Main()
    {
        var dictionary = new StronglyTypedNamedPersistedDictionary(new Dictionary<string, object?>());
        AssertEqual(float.NaN, dictionary.FloatNaN, nameof(dictionary.FloatNaN));
        AssertEqual(float.PositiveInfinity, dictionary.FloatPositiveInfinity, nameof(dictionary.FloatPositiveInfinity));
        AssertEqual(float.NegativeInfinity, dictionary.FloatNegativeInfinity, nameof(dictionary.FloatNegativeInfinity));
        AssertEqual(float.MinValue, dictionary.FloatMinValue, nameof(dictionary.FloatMinValue));
        AssertEqual(float.MaxValue, dictionary.FloatMaxValue, nameof(dictionary.FloatMaxValue));
        AssertEqual(float.Epsilon, dictionary.FloatEpsilon, nameof(dictionary.FloatEpsilon));
        AssertEqual(double.NaN, dictionary.DoubleNaN, nameof(dictionary.DoubleNaN));
        AssertEqual(double.PositiveInfinity, dictionary.DoublePositiveInfinity, nameof(dictionary.DoublePositiveInfinity));
        AssertEqual(double.NegativeInfinity, dictionary.DoubleNegativeInfinity, nameof(dictionary.DoubleNegativeInfinity));
        AssertEqual(double.MinValue, dictionary.DoubleMinValue, nameof(dictionary.DoubleMinValue));
        AssertEqual(double.MaxValue, dictionary.DoubleMaxValue, nameof(dictionary.DoubleMaxValue));
        AssertEqual(double.Epsilon, dictionary.DoubleEpsilon, nameof(dictionary.DoubleEpsilon));
        AssertEqual(long.MinValue, dictionary.LongMinValue, nameof(dictionary.LongMinValue));
        AssertEqual(long.MaxValue, dictionary.LongMaxValue, nameof(dictionary.LongMaxValue));
        AssertEqual(ulong.MaxValue, dictionary.ULongMaxValue, nameof(dictionary.ULongMaxValue));
        AssertEqual(DayOfWeek.Friday, dictionary.EnumDefault, nameof(dictionary.EnumDefault));

        dictionary.FloatNaN = 1.25f;
        AssertEqual(1.25f, dictionary.FloatNaN, nameof(dictionary.FloatNaN));
        dictionary.DoublePositiveInfinity = -12.5;
        AssertEqual(-12.5, dictionary.DoublePositiveInfinity, nameof(dictionary.DoublePositiveInfinity));

        Console.Write(dictionary.Property1);
    }

    private static void AssertEqual<T>(T expected, T actual, string propertyName)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{propertyName}: expected {expected}, got {actual}.");
        }
    }
}