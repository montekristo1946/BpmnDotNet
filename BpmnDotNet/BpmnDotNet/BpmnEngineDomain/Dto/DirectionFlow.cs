namespace BpmnDotNet.BpmnEngineDomain.Dto;

using System.Collections.Concurrent;
using BpmnDotNet.BpmnEngineDomain.Abstractions;

/// <summary>
/// DTO, описывающая связь потока и ресурса.
/// </summary>
/// <param name="IdFlow">Идентификатор потока.</param>
/// <param name="IdResource">Идентификатор ресурса.</param>
internal record DirectionFlow(string IdFlow, string IdResource);