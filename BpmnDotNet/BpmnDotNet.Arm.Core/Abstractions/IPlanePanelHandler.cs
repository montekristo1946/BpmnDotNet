using System.Runtime.CompilerServices;
using BpmnDotNet.Common.BPMNDiagram;

[assembly: InternalsVisibleTo("BpmnDotNet.Arm.Web")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace BpmnDotNet.Arm.Core.Abstractions;

/// <summary>
/// Handler для работы PlanePane.
/// </summary>
internal interface IPlanePanelHandler
{
    /// <summary>
    /// Вернет BpmnPlane для отрисовки.
    /// </summary>
    public Task <BpmnPlane> GetPlane();
}