namespace BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// Стрелочка (Flow).
/// </summary>
internal class Flow
{
    /// <summary>
    /// Gets уникальный идентификатор.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets iD узла из которого исходит стрелка.
    /// </summary>
    public string SourceId { get; init; } = string.Empty;

    /// <summary>
    /// Gets iD узла в который приходит стрелка.
    /// </summary>
    public string TargetId { get; init; } = string.Empty;
}