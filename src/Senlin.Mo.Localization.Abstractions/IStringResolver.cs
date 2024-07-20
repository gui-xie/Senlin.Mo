namespace Senlin.Mo.Localization.Abstractions;

/// <summary>
/// Localization string
/// </summary>
public interface IL : ILStringResolver
{
    private class LImpl : IL
    {
        private readonly LStringResolver _lStringResolver;

        public LImpl(LStringResolver lStringResolver)
        {
            _lStringResolver = lStringResolver;
        }
        
        public string this[LString lString] => _lStringResolver[lString];
    }
}

/// <summary>
/// Localization string resolver
/// </summary>
public interface ILStringResolver
{
    /// <summary>
    /// Get localization string
    /// </summary>
    /// <param name="lString"></param>
    string this[LString lString] { get; }
}

/// <summary>
/// Localization string resolver(For specific module)
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ILStringResolver<T> : ILStringResolver
{
    
}