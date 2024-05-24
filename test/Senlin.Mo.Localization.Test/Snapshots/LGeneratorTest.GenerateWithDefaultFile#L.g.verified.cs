//HintName: L.g.cs
#nullable enable
using Senlin.Mo.Localization.Abstractions;
namespace ProjectA
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Senlin.Mo.Localization", "1.0.1.0")]
    public static partial class L
    {
        // this can be used as default resource
        public static readonly Dictionary<string, string> LStringSource = new Dictionary<string, string>
        {
            {"namespace", "Test"},
            {"name", "Name"},
            {"ageIs", "Age is {age}"},
        };

        /// <summary>
        /// Test
        /// </summary>
        public static LString Namespace = new LString("namespace");

        /// <summary>
        /// Name
        /// </summary>
        public static LString Name = new LString("name");

        /// <summary>
        /// Age is {age}
        /// </summary>
        public static LString AgeIs(string age)
        {
            return new LString("ageIs", new []{
                new KeyValuePair<string, string>("age", age),
            });
        }
    }
}
#nullable restore