using System.Threading.Tasks;
using BpmnDotNet.Common.Dto;

namespace BpmnDotNet.Common.Abstractions;

/// <summary>
/// Клиент для записи в Elastic
/// </summary>
public interface IElasticClient
{
    /// <summary>
    /// Сохраняет модели истории проходов по блокам.
    /// </summary>
    /// <param name="historyNodeState"></param>
    public Task<bool> SetHistoryNodeStateAsync(HistoryNodeState historyNodeState);
}