using Demo;

public class Program
{
    public static void Main()
    {
        var dictionary = new StronglyTypedNamedPersistedDictionary(new Dictionary<string, object?>());
        Console.Write(dictionary.Property1);
    }
}