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

    [System.ComponentModel.DefaultValue("Hello Roslyn!")]
    string Property4 { get; set; }

    CultureInfo Property5 { get; set; }
    
    [System.ComponentModel.DefaultValue(null)]
    TimeSpan Property6 { get; set; }
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

    public CultureInfo Property5
    {
        get => CultureInfo.GetCultureInfo(Get());
        set => Set(value.Name);
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
    public string Property4
    {
        get => GetOrDefault("Hello Roslyn!");
        set => Set(value);
    }
    public string Property6
    {
        get => GetOrDefault<System.TimeSpan>(default(System.TimeSpan));
        set => Set(value);
    }
}

public abstract class GeneratedBase
{
    private sealed class StronglyTypedKvpWrapper : IStronglyTypedKeyValuePairAccessor
    {
        private readonly global::System.Collections.Generic.IDictionary<string, object> dictionary;

        public StronglyTypedKvpWrapper(global::System.Collections.Generic.IDictionary<string, object> dictionary)
        {
            this.dictionary = dictionary;
        }

        public T Get<T>(string name) where T : unmanaged
        {
            return (T)dictionary[name];
        }

        public string Get(string name)
        {
            return (string)dictionary[name];
        }

        public bool TryGet<T>(string name, out T value) where T : unmanaged
        {
            var result = dictionary.TryGetValue(name, out object? v);
            value = v is not null ? (T)v : default;
            return result;
        }

        public bool TryGet(string name, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
        {
            var result = dictionary.TryGetValue(name, out object? v);
            value = (string?)v;
            return result;
        }

        public void Set<T>(string name, T value) where T : unmanaged
        {
            dictionary[name] = value;
        }

        public void Set(string name, string value)
        {
            dictionary[name] = value;
        }
    }

    private readonly IStronglyTypedKeyValuePairAccessor stronglyTypedKeyValuePairAccessor;

    private protected GeneratedBase(IStronglyTypedKeyValuePairAccessor stronglyTypedKeyValuePairAccessor)
    {
        this.stronglyTypedKeyValuePairAccessor = stronglyTypedKeyValuePairAccessor;
    }

    private protected GeneratedBase(global::System.Collections.Generic.IDictionary<string, object> dictionary)
        : this(new StronglyTypedKvpWrapper(dictionary))
    {

    }

    protected T Get<T>([global::System.Runtime.CompilerServices.CallerMemberName] string name = null!) where T : unmanaged
    {
        return stronglyTypedKeyValuePairAccessor.Get<T>(name);
    }

    protected string Get([global::System.Runtime.CompilerServices.CallerMemberName] string name = null!)
    {
        return stronglyTypedKeyValuePairAccessor.Get(name);
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected virtual T GetOrDefault<T>(T defaultValue, [global::System.Runtime.CompilerServices.CallerMemberName] string name = null!) where T : unmanaged
    {
        return stronglyTypedKeyValuePairAccessor.TryGet(name, out T result) ? result : defaultValue;
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected virtual string GetOrDefault(string defaultValue, [global::System.Runtime.CompilerServices.CallerMemberName] string name = null!)
    {
        return stronglyTypedKeyValuePairAccessor.TryGet(name, out string? result) ? result : defaultValue;
    }

    protected void Set<T>(T value, [global::System.Runtime.CompilerServices.CallerMemberName] string name = null!) where T : unmanaged
    {
        stronglyTypedKeyValuePairAccessor.Set(name, value);
    }

    protected void Set(string value, [global::System.Runtime.CompilerServices.CallerMemberName] string name = null!)
    {
        stronglyTypedKeyValuePairAccessor.Set(name, value);
    }
}

public abstract class GeneratedBase<TInterface> : GeneratedBase
{
    private protected GeneratedBase(IStronglyTypedKeyValuePairAccessor stronglyTypedKeyValuePairAccessor) : base(stronglyTypedKeyValuePairAccessor)
    {
    }

    private protected GeneratedBase(global::System.Collections.Generic.IDictionary<string, object> dictionary) : base(dictionary)
    {
    }

    public TProperty Get<TProperty>(global::System.Linq.Expressions.Expression<Func<TInterface, TProperty>> expression)
        where TProperty : unmanaged
    {
        return base.Get<TProperty>(GetMemberName(expression));
    }

    public string Get(global::System.Linq.Expressions.Expression<Func<TInterface, string>> expression)
    {
        return base.Get(GetMemberName(expression));
    }

    public void Set<TProperty>(global::System.Linq.Expressions.Expression<Func<TInterface, TProperty>> expression, TProperty value)
        where TProperty : unmanaged
    {
        base.Set<TProperty>(value, GetMemberName(expression));
    }

    public void Set(global::System.Linq.Expressions.Expression<Func<TInterface, string>> expression, string value)
    {
        base.Set(value, GetMemberName(expression));
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


