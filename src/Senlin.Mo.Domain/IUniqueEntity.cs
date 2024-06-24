using System.Linq.Expressions;

namespace Senlin.Mo.Domain;

/// <summary>
///  used for unique judgment
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IUniqueEntity<T> where T: class
{
    /// <summary>
    ///  used for unique judgment
    /// </summary>
    Expression<Func<T, bool>> GetIsRepeatExpression();
}