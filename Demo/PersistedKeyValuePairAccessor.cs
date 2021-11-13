using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public bool TryGet<T>(string name, out T value) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void Set<T>(string name, T value) where T : unmanaged
        {
            throw new NotImplementedException();
        }
    }
}
