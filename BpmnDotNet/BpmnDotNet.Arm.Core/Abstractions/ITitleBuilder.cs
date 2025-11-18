namespace BpmnDotNet.Arm.Core.Abstractions;

public interface ITitleBuilder<out T> where T : new()
{
    public T AddTitle(string? titleText);
}