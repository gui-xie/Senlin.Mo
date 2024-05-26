namespace Senlin.Mo.Repository.Abstractions;

/// <summary>
/// Connection String
/// </summary>
/// <param name="value"></param>
/// <typeparam name="T"></typeparam>
public class ConnectionString<T>(string value) where T : class, IRepositoryDbContext
{
    private readonly string _value = value;

    /// <summary>
    /// Convert to string implicitly
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static implicit operator string(ConnectionString<T> connectionString) => connectionString._value;
}

