//HintName: LResource.g.cs
#nullable enable
using Senlin.Mo.Localization.Abstractions;
namespace ProjectA
{
    public abstract class LResource: ILResource
    {
        public abstract string Culture { get; }

        protected abstract string Name { get; }

        protected abstract string AgeIs { get; }

        public Dictionary<string, string> GetResource() => new()
        {
            { "name", Name },
            { "ageIs", AgeIs },
        };
    }
}
#nullable restore