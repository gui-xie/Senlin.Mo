﻿//HintName: L.g.cs
#nullable enable
using Senlin.Mo.Localization.Abstractions;
namespace Test
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Senlin.Mo.Localization", "1.0.0.0")]
    public static partial class L
    {
        public const string Directory = "L";

        /// <summary>
        /// Name
        /// </summary>
        public static LocalizationString Name = new LocalizationString("name");

        /// <summary>
        /// Age is {age}
        /// </summary>
        public static LocalizationString AgeIs(string age)
        {
            return new LocalizationString("ageIs", new []{
                new KeyValuePair<string, string>("age", age),
            });
        }
    }
}
#nullable restore