namespace BpmnDotNet.Dto;

/// <summary>
/// Описывает Данные для узла Bpmn.
/// </summary>
public class DescriptionData
{
    /// <summary>
    /// Gets id.
    /// </summary>
    public string Id => TaskDefinitionId;

    /// <summary>
    /// Gets iD Bpmn узла.
    /// </summary>
    public string TaskDefinitionId { get; init; } = string.Empty;

    /// <summary>
    /// Gets описание узла.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}