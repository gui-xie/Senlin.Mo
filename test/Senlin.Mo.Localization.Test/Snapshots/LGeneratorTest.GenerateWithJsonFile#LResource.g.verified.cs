//HintName: LResource.g.cs
#nullable enable
using Senlin.Mo.Localization.Abstractions;
namespace ProjectA
{
    /// <summary>
    /// Localization resource base class
    /// </summary>
    public abstract class LResource: ILResource
    {

        /// <summary>
        /// Culture
        /// </summary>
        public abstract string Culture { get; }

        /// <summary>
        /// Name
        /// </summary>
        protected abstract string Name { get; }

        /// <summary>
        /// Age is {age}
        /// </summary>
        protected abstract string AgeIs { get; }

        /// <summary>
        /// Age is {age}
        /// </summary>
        protected abstract string AgeIsEscape { get; }


        /// <summary>
        /// Get localization resource
        /// </summary>
        public Dictionary<string, string> GetResource() => new()
        {
            { "name", Name },
            { "ageIs", AgeIs },
            { "ageIsEscape", AgeIsEscape },
        };
    }
}
#nullable restore