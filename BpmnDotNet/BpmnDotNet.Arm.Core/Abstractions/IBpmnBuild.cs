namespace BpmnDotNet.Arm.Core.Abstractions;

public interface IBpmnBuild<out T> where T : new()
{
    public string Build();

    public static T Create()
    {
        return new T();
    }

    public T AddChild(string childElement);
}