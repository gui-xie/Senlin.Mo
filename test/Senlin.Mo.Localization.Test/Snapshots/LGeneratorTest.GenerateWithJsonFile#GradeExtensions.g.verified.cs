//HintName: GradeExtensions.g.cs
#nullable enable
using Senlin.Mo.Localization.Abstractions;
using ProjectA;

namespace ProjectA
{
    /// <summary>
    /// Grade localization string extensions
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Senlin.Mo.Localization", "1.0.20.0")]
    public static partial class GradeExtensions
    {
        /// <summary>
        /// Convert Grade to localization string
        /// </summary>
        public static LString ToLString(this Grade grade)
        {
            return grade switch
            {
                Grade.Excellent => L.Grade_Excellent,
                Grade.Good => L.Grade_Good,
                Grade.Pass => L.Grade_Pass,
                Grade.Fail => L.Grade_Fail,
                _ => LString.Empty
            };
        }
    }
}
#nullable restore