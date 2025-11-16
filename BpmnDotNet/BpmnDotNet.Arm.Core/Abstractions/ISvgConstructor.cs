using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Common.Entities;

namespace BpmnDotNet.Arm.Core.Abstractions;

/// <summary>
/// Отрисовка svg.
/// </summary>
public interface ISvgConstructor
{
    /// <summary>
    /// Создать раскрашенный план bpmn.
    /// </summary>
    /// <param name="plane">План в объекте виде.</param>
    /// <param name="nodeJobStatus">Состояние узлов на Bpmn схеме.</param>
    /// <param name="sizeWindows">Размер окна под которое подогнать svg.</param>
    /// <param name="descriptions">Текст всплывающие подсказки.</param>
    /// <returns></returns>
    Task<string> CreatePlane(BpmnPlane plane, NodeJobStatus[] nodeJobStatus, SizeWindows sizeWindows,DescriptionData[]  descriptions);
}