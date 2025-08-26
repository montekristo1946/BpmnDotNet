namespace BpmnDotNet.Arm.Core.Abstractions;

public interface IBpmnBuild<T> where T : new()
{
    public string Build();

    public static T Create()
    {
        return new T();
    }

    public void AddChild(string childElement);
}