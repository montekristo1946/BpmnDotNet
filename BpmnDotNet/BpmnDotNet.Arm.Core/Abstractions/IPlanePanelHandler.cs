using System.Runtime.CompilerServices;
using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.BPMNDiagram;

[assembly: InternalsVisibleTo("BpmnDotNet.Arm.Web")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace BpmnDotNet.Arm.Core.Abstractions;

/// <summary>
///     Handler для работы PlanePane.
/// </summary>
internal interface IPlanePanelHandler
{
    /// <summary>
    ///     Вернет BpmnPlane для отрисовки.
    /// </summary>
    /// <param name="idProcess"></param>
    /// <param name="sizeWindows"></param>
    public Task<string> GetPlane(string idProcess, SizeWindows sizeWindows);
}