﻿//HintName: L.g.cs
#nullable enable
using Senlin.Mo.Localization.Abstractions;
namespace ProjectA
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Senlin.Mo.Localization", "1.0.1.0")]
    public static partial class L
    {
        /// <summary>
        /// Name
        /// </summary>
        public static LString Name = new LString("name", "Name");

        /// <summary>
        /// Age is {age}
        /// </summary>
        public static LString AgeIs(string age)
        {
            return new LString(
                "ageIs",
                "Age is {age}",
                new []
                {
                    new KeyValuePair<string, string>("age", age),
                }
            );
        }
    }
}
#nullable restore