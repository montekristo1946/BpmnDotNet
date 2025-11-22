namespace BpmnDotNet.Arm.Core.UiDomain.Abstractions;

using BpmnDotNet.Arm.Core.UiDomain.Dto;

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