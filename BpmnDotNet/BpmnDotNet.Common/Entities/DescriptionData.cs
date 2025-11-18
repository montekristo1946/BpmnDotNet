namespace BpmnDotNet.Common.Entities;

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
    /// Gets or sets описание узал.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}