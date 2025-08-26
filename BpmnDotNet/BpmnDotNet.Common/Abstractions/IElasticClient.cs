using System.Threading.Tasks;
using BpmnDotNet.Common.Dto;

namespace BpmnDotNet.Common.Abstractions;

/// <summary>
///     Клиент для записи в Elastic
/// </summary>
public interface IElasticClient
{
    /// <summary>
    ///     Сохраняет модели истории проходов по блокам.
    /// </summary>
    /// <param name="document"></param>
    public Task<bool> SetDataAsync<T>(T document);

    /// <summary>
    ///     Получить данные по ID.
    /// </summary>
    /// <param name="id"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<T?> GetDataFromIdAsync<T>(string id);

    /// <summary>
    ///     Запросить последних count данных.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="valueFind"></param>
    /// <returns></returns>
    public Task<HistoryNodeState[]> GetLastDataAsync(int count, string valueFind);

    /// <summary>
    ///     Пагинация для HistoryNodeState.
    /// </summary>
    /// <param name="valueFind"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public Task<HistoryNodeState[]> SearchWithPaginationAsync(
        string valueFind,
        int pageNumber = 1,
        int pageSize = 10);

    /// <summary>
    ///     Получить общее количество.
    /// </summary>
    /// <param name="valueFind"></param>
    /// <returns></returns>
    public Task<long> GetHistoryNodeStateCountAsync(string valueFind);
}