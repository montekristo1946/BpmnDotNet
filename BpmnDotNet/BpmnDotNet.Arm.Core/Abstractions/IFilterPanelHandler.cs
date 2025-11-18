using BpmnDotNet.Arm.Core.Dto;

namespace BpmnDotNet.Arm.Core.Abstractions;

/// <summary>
/// Интерфейс сервиса для панели FilterPane
/// </summary>
public interface IFilterPanelHandler
{
    /// <summary>
    /// Получить Инфоу по процессам.
    /// </summary>
    /// <returns></returns>
    public Task<ProcessDataFilterPanel[]> GetAllProcessIdAsync();
}