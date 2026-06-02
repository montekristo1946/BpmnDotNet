namespace BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// Стрелочка (Flow).
/// </summary>
internal class Flow
{
    /// <summary>
    /// Gets or sets уникальный идентификатор.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets iD узла из которого исходит стрелка.
    /// </summary>
    public string SourceId { get; set; }

    /// <summary>
    /// Gets or sets iD узла в который приходит стрелка.
    /// </summary>
    public string TargetId { get; set; }
}