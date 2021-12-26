using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    [StronglyTypedDictionary(targetType: typeof(IMyInterface), supportsDefaultValues: true)]
    internal partial class StronglyTypedNamedPersistedDictionary : IMyInterface
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
}
