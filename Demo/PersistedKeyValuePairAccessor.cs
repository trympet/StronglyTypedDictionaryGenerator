using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    internal class PersistedKeyValuePairAccessor : IStronglyTypedKeyValuePairAccessor
    {
        private readonly Dictionary<string, object> _myPersistedKeyValueStore = new Dictionary<string, object>();

        public T Get<T>(string name) where T : unmanaged
        {
            return (T)_myPersistedKeyValueStore[name];
        }

        public string Get(string name)
        {
            return (string)_myPersistedKeyValueStore[name];
        }

        public bool TryGet<T>(string name, out T value) where T : unmanaged
        {
            var result = _myPersistedKeyValueStore.TryGetValue(name, out object? v);
            value = v is not null ? (T)v : default;
            return result;
        }

        public bool TryGet(string name, [NotNullWhen(true)] out string? value)
        {
            var result = _myPersistedKeyValueStore.TryGetValue(name, out object? v);
            value = (string?)v;
            return result;
        }

        public void Set<T>(string name, T value) where T : unmanaged
        {
            _myPersistedKeyValueStore[name] = value;
        }

        public void Set(string name, string value)
        {
            _myPersistedKeyValueStore[name] = value;
        }
    }
}
