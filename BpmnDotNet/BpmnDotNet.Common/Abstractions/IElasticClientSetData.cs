using System.Threading.Tasks;

namespace BpmnDotNet.Common.Abstractions;

/// <summary>
/// Запись данных 
/// </summary>
public interface IElasticClientSetDataAsync
{
    /// <summary>
    ///     Сохраняет модели истории проходов по блокам.
    /// </summary>
    /// <param name="document"></param>
    public Task<bool> SetDataAsync<T>(T document);
}