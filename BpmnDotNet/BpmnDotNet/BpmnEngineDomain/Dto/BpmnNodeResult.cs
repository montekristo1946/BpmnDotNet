namespace BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// Состояние обработки node.
/// </summary>
internal record BpmnNodeResult()
{
    /// <summary>
    /// Gets следующие token nods для обработки.
    /// </summary>
    public IEnumerable<Token> Tokens { get; init; } = [];

    /// <summary>
    /// Gets статус выполнения.
    /// </summary>
    public StatusNode Status { get; init; } = StatusNode.None;
}