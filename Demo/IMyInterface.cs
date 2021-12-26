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
    }
}
