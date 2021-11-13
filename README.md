# StronglyTypedDictionaryGenerator
Roslyn powered source generator for strongly typed dictionaries.

Easily turn an interface into a strongly typed dictionary. Use it for persisting data, interop, serialization, config files, etc.

## Generated source
#### Sample user defined input:
```csharp
interface IMyInterface
{
    [System.ComponentModel.DefaultValue(69)]
    int Property1 { get; set; }

    bool Property2 { get; set; }

    [System.ComponentModel.DefaultValue(true)]
    bool Property3 { get; set; }

    bool Property4 { get; set; }
}

[StronglyTypedDictionary(targetType: typeof(IMyInterface), supportsDefaultValues: true)]
internal partial class MyStronglyTypedDictionary : IMyInterface
{
    private bool someCondition;

    protected override T GetOrDefault<T>(T defaultValue, [CallerMemberName] string name = null!)
    {
        switch (name)
        {
            case nameof(IMyInterface.Property3) when someCondition:
                defaultValue = (T)(object)false;
                break;
            default:
                break;
        }

        return base.GetOrDefault(defaultValue, name);
    }
}
```

#### Sample generated output:
```csharp
internal partial class MyStronglyTypedDictionary : GeneratedBase<global::Demo.IMyInterface>, global::Demo.IMyInterface
{
    public MyStronglyTypedDictionary(IStronglyTypedKeyValuePairAccessor stronglyTypedKeyValuePairAccessor) : base(stronglyTypedKeyValuePairAccessor)
    {
    }

    public MyStronglyTypedDictionary(global::System.Collections.Generic.IDictionary<string, object> dictionary) : base(dictionary)
    {
    }

    public int Property1
    {
        get => GetOrDefault<int>(69);
        set => Set(value);
    }
    public bool Property2
    {
        get => Get<bool>();
        set => Set(value);
    }
    public bool Property3
    {
        get => GetOrDefault<bool>(true);
        set => Set(value);
    }
    public bool Property4
    {
        get => Get<bool>();
        set => Set(value);
    }
}

public abstract class GeneratedBase<TInterface>
{
    private class StronglyTypedKvpWrapper : IStronglyTypedKeyValuePairAccessor
    {
        private readonly IDictionary<string, object> dictionary;

        public StronglyTypedKvpWrapper(IDictionary<string, object> dictionary)
        {
            this.dictionary = dictionary;
        }

        public T Get<T>(string name) where T : unmanaged
        {
            return (T)dictionary[name];
        }

        public bool TryGet<T>(string name, out T value) where T : unmanaged
        {
            object v;
            var result = dictionary.TryGetValue(name, out v);
            value = (T)v;
            return result;
        }

        public void Set<T>(string name, T value) where T : unmanaged
        {
            dictionary[name] = value;
        }
    }

    private readonly IStronglyTypedKeyValuePairAccessor stronglyTypedKeyValuePairAccessor;

    private protected GeneratedBase(IStronglyTypedKeyValuePairAccessor stronglyTypedKeyValuePairAccessor)
    {
        this.stronglyTypedKeyValuePairAccessor = stronglyTypedKeyValuePairAccessor;
    }

    private protected GeneratedBase(IDictionary<string, object> dictionary)
        : this(new StronglyTypedKvpWrapper(dictionary))
    {
        
    }

    public TProperty Get<TProperty>(global::System.Linq.Expressions.Expression<Func<TInterface, TProperty>> expression)
        where TProperty : unmanaged
    {
        return Get<TProperty>(GetMemberName(expression));
    }

    public void Set<TProperty, TValue>(global::System.Linq.Expressions.Expression<Func<TInterface, TValue>> expression, TProperty value)
        where TProperty : unmanaged, TValue
    {
        stronglyTypedKeyValuePairAccessor.Set(GetMemberName(expression), value);
    }

    protected T Get<T>([global::System.Runtime.CompilerServices.CallerMemberName] string name = null!) where T : unmanaged
    {
        return stronglyTypedKeyValuePairAccessor.Get<T>(name);
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected virtual T GetOrDefault<T>(T defaultValue, [global::System.Runtime.CompilerServices.CallerMemberName] string name = null!) where T : unmanaged
    {
        return stronglyTypedKeyValuePairAccessor.TryGet(name, out T result) ? result : defaultValue;
    }

    protected void Set<T>(T value, [global::System.Runtime.CompilerServices.CallerMemberName] string name = null!) where T : unmanaged
    {
        stronglyTypedKeyValuePairAccessor.Set(name, value);
    }

    private static string GetMemberName<TProperty>(global::System.Linq.Expressions.Expression<Func<TInterface, TProperty>> expression)
    {
        return expression.Body switch
        {
            global::System.Linq.Expressions.MemberExpression m => m.Member.Name,
            global::System.Linq.Expressions.UnaryExpression u when u.Operand is global::System.Linq.Expressions.MemberExpression m => m.Member.Name,
            _ => throw new global::System.NotImplementedException($"Invalid expression: {expression.GetType()}. Exprected accessor."),
        };
    }
}
```


