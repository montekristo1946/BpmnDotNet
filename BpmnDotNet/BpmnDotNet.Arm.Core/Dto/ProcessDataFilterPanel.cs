namespace BpmnDotNet.Arm.Core.Dto;

/// <summary>
/// ДТО для отображения в панели FilterPanel.
/// </summary>
public record ProcessDataFilterPanel
{
    /// <summary>
    /// Gets iD из схемы процесса.
    /// </summary>
    public string IdBpmnProcess { get; init; } = string.Empty;

    /// <summary>
    /// Gets имя из схемы процесса.
    /// </summary>
    public string NameBpmnProcess { get; init; } = string.Empty;
}