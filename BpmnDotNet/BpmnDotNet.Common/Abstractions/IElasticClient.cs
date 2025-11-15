namespace BpmnDotNet.Common.Abstractions;

using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using BpmnDotNet.Common.Dto;

/// <summary>
///     Клиент для записи в Elastic.
/// </summary>
public interface IElasticClient : IElasticClientSetDataAsync
{
    /// <summary>
    ///     Получить данные по ID.
    /// </summary>
    /// <param name="id">ID объекта в базе данных.</param>
    /// <param name="sourceExcludes">Source которые запросим.</param>
    /// <typeparam name="T">Тип индекса в базе данных.</typeparam>
    /// <returns>ДТО которую запросили со всеми заполненными полями.</returns>
    public Task<T?> GetDataFromIdAsync<T>(string id, string[]? sourceExcludes = null);

    /// <summary>
    /// Получить все записи по полю TField из TIndex.
    /// </summary>
    /// <param name="searchFields">Поля которые нужно выбрать.</param>
    /// <param name="maxCountElements">Масимальное количество возращаемых обьектов.</param>
    /// <typeparam name="TIndex">Тип индекса.</typeparam>
    /// <returns>Массив полей.</returns>
    public Task<TIndex[]> GetAllFieldsAsync<TIndex>(string [] searchFields, int maxCountElements)
        where TIndex : class;

    /// <summary>
    /// Общее количество полей, с фильтром ProcessStatus.
    /// </summary>
    /// <param name="idBpmnProcess">ID BpmnProcess.</param>
    /// <param name="processStatus">Массив ProcessStatus.</param>
    /// <param name="sizeSample">Количество, которое будем анализировать.</param>
    /// <returns>Найденное количество.</returns>
    public Task<int> GetCountHistoryNodeStateAsync(string idBpmnProcess, string[] processStatus, int sizeSample = 10000);

    /// <summary>
    /// Выдача результатов пагинация, с фильтром ProcessStatus.
    /// </summary>
    /// <param name="idBpmnProcess">ID BpmnProcess.</param>
    /// <param name="from">С какого элемента.</param>
    /// <param name="size">Сколько элементов.</param>
    /// <param name="processStatus">Массив ProcessStatus.</param>
    /// <returns>Массив HistoryNodeState.</returns>
    public Task<HistoryNodeState[]> GetHistoryNodeStateAsync(string idBpmnProcess, int from, int size, string[] processStatus);

    /// <summary>
    /// Поиск HistoryNodeState по маске в поле Token.
    /// </summary>
    /// <param name="idBpmnProcess">ID BpmnProcess.</param>
    /// <param name="mask">Маска поиска формата elastic.</param>
    /// <param name="sizeSample">Количество выдаваемого результата.</param>
    /// <returns>Массив HistoryNodeState.</returns>
    public Task<HistoryNodeState[]> GetHistoryNodeFromTokenMaskAsync(string idBpmnProcess, string mask, int sizeSample = 100);
}