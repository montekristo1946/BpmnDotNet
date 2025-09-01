using System;
using System.Runtime.InteropServices.Marshalling;
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
    /// <param name="sourceExcludes"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<T?> GetDataFromIdAsync<T>(string id, string[]? sourceExcludes = null);


    /// <summary>
    /// Получить все записи по полю TField из TIndex. 
    /// </summary>
    /// <typeparam name="TIndex"></typeparam>
    /// <typeparam name="TField"></typeparam>
    /// <returns></returns>
    public Task<TField[]> GetAllFieldsAsync<TIndex, TField>(string nameField, int maxCountElements) where TIndex : class;

    /// <summary>
    /// Сколько всего групп конкретного процесса для пагинации. В расчет берется первая 1000;
    /// </summary>
    /// <param name="idActiveProcess"></param>
    /// <returns></returns>
    public Task<int> GetAllGroupFromTokenAsync(string idActiveProcess);
    
    /// <summary>
    /// Получить 
    /// </summary>
    /// <param name="idActiveProcess"></param>
    /// <param name="afterKeyValueparam>
    /// <param name="countLineOnePage"></param>
    /// <returns></returns>
    public Task<string[]> GetIdHistoryNodeStateAsync(string idActiveProcess, string afterKeyValue,
        int countLineOnePage);
}