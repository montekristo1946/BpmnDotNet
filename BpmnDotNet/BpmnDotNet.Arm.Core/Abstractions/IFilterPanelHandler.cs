namespace BpmnDotNet.Arm.Core.Abstractions;

using BpmnDotNet.Arm.Core.Dto;

/// <summary>
/// Интерфейс сервиса для панели FilterPane.
/// </summary>
internal interface IFilterPanelHandler
{
    /// <summary>
    /// Получить Инфу по процессам.
    /// </summary>
    /// <returns>ДТО панели FilterPanel.</returns>
    public Task<ProcessDataFilterPanel[]> GetAllProcessIdAsync();
}