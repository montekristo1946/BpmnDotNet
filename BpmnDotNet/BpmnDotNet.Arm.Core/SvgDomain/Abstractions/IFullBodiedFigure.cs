namespace BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Конструктор Полнотелых фигур.
/// </summary>
/// <typeparam name="T">Тип блока.</typeparam>
internal interface IFullBodiedFigure<out T> : IColorBuilder<T>, ITitleBuilder<T>, IBpmnBuild<T>, IOffsetsPosition<T>
    where T : new()
{
}