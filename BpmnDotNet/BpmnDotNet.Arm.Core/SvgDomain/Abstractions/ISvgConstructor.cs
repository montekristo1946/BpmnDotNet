namespace BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

using BpmnDotNet.Arm.Core.Common;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Common.Entities;

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
    /// <returns>SVG для отрисовки.</returns>
    Task<string> CreatePlaneAsync(
        BpmnPlane plane,
        NodeJobStatus[] nodeJobStatus,
        SizeWindows sizeWindows,
        DescriptionData[] descriptions);
}