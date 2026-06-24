namespace BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// Токен для переходов по ветвям.
/// </summary>
internal class Token
{
    /// <summary>
    /// Gets уникальный идентификатор.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets id node которую нужно выполнить.
    /// </summary>
    public string CurrentNodeId { get; set; } = string.Empty;
}