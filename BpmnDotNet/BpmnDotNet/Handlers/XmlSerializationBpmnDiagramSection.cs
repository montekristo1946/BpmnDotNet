namespace BpmnDotNet.Handlers;

using System.Xml;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BPMNDiagram;
using BpmnDotNet.BPMNDiagram.Abstractions;
using BpmnDotNet.Utils;

/// <inheritdoc />
public class XmlSerializationBpmnDiagramSection : IXmlSerializationBpmnDiagramSection
{
    /// <inheritdoc />
    public BpmnPlane LoadXmlBpmnDiagram(string pathDiagram)
    {
        if (string.IsNullOrWhiteSpace(pathDiagram))
        {
            throw new ArgumentNullException(nameof(pathDiagram));
        }

        var ret = new XmlDocument();
        ret.Load(pathDiagram);

        var retValue = ParsingXml(ret);
        return retValue;
    }

    private BpmnPlane ParsingXml(XmlDocument xmlDoc)
    {
        ArgumentNullException.ThrowIfNull(xmlDoc);

        XmlNode? root = xmlDoc.DocumentElement;
        if (root == null || root.Name != Constants.BpmnRootName)
        {
            throw new InvalidOperationException($"Not find {Constants.BpmnRootName}");
        }

        var bpmnDiagram = root.ChildNodes.Cast<XmlNode>().FirstOrDefault(p => p.Name == Constants.BpmnDiagramName);
        if (bpmnDiagram is null)
        {
            throw new InvalidOperationException($"Not find {Constants.BpmnDiagramName}");
        }

        var bpmnPlane = bpmnDiagram.ChildNodes.Cast<XmlNode>()
            .FirstOrDefault(p => p.Name == Constants.BpmnBpmnPlaneName);
        if (bpmnPlane is null)
        {
            throw new InvalidOperationException($"Not find {Constants.BpmnBpmnPlaneName}");
        }

        var processBlock = root.ChildNodes.Cast<XmlNode>().FirstOrDefault(p => p.Name == Constants.BpmnProcessName);
        if (processBlock is null)
        {
            throw new InvalidOperationException($"Not find {Constants.BpmnProcessName}");
        }

        var idBpmnPlane = GetId(bpmnPlane);
        var bpmnElementPlane = GetBpmnElement(bpmnPlane);

        var shapes = GetShapesFromPlane(bpmnPlane, idBpmnPlane);
        shapes = FillTypeAndName(shapes, processBlock);
        shapes = FillBpmnText(shapes, processBlock);
        var namePrecess = GetNameProcess(processBlock, bpmnElementPlane);

        return new BpmnPlane
        {
            IdBpmnProcess = bpmnElementPlane,
            Shapes = shapes,
            Name = namePrecess,
        };
    }

    private IBpmnShape[] FillBpmnText(IBpmnShape[] shapes, XmlNode processBlock)
    {
        var bounds = processBlock.ChildNodes
            .Cast<XmlNode>()
            .Select(p =>
            {
                var id = GetId(p);
                var text = p.ChildNodes.Cast<XmlNode>().FirstOrDefault(p => p.Name == Constants.Text)?.InnerText;
                return (id, text);
            })
            .Where(p => !string.IsNullOrWhiteSpace(p.text))
            .ToArray();

        var retArr = shapes.Select(iBpmnShape =>
        {
            var element = bounds.FirstOrDefault(p => p.id == iBpmnShape.BpmnElement);

            return iBpmnShape.TypeBpmnShape switch
            {
                BpmnShapeType.BpmnShape => (BpmnShape)iBpmnShape with
                {
                    BpmnText = element.text!,
                },
                BpmnShapeType.BpmnEdge => iBpmnShape,
                _ => throw new ArgumentOutOfRangeException($"[FillTypeAndName] fail Type shape: {iBpmnShape.TypeBpmnShape}"),
            };
        }).ToArray();

        return retArr;
    }

    private string GetNameProcess(XmlNode? processBlock, string idBpmnPlane)
    {
        var processName = processBlock?.Attributes?[Constants.BpmnAttributesName]?.Value;

        return processName ??
               throw new InvalidDataException($"Not Find Get Name Process from:{idBpmnPlane}");
    }

    private IBpmnShape[] FillTypeAndName(IBpmnShape[] shapes, XmlNode processBlock)
    {
        var bounds = processBlock.ChildNodes
            .Cast<XmlNode>()
            .Select(p =>
            {
                var type = GetTypeNode(p.Name);
                var id = GetId(p);
                var name = p.Attributes?[Constants.BpmnAttributesName]?.Value ?? string.Empty;
                return (id, type, name);
            }).ToArray();

        var retArr = shapes.Select(iBpmnShape =>
        {
            var element = bounds.FirstOrDefault(p => p.id == iBpmnShape.BpmnElement);

            return iBpmnShape.TypeBpmnShape switch
            {
                BpmnShapeType.BpmnShape => (BpmnShape)iBpmnShape with
                {
                    Name = element.name, Type = element.type,
                },
                BpmnShapeType.BpmnEdge => (IBpmnShape)((BpmnEdge)iBpmnShape with
                {
                    Name = element.name, Type = element.type,
                }),
                _ => throw new ArgumentOutOfRangeException($"[FillTypeAndName] fail Type shape: {iBpmnShape.TypeBpmnShape}"),
            };
        }).ToArray();

        return retArr;
    }

    private ElementType GetTypeNode(string shapeName)
    {
        return shapeName switch
        {
            Constants.BpmnStartEventName => ElementType.StartEvent,
            Constants.BpmnSequenceFlowName => ElementType.SequenceFlow,
            Constants.BpmnEndEventName => ElementType.EndEvent,
            Constants.BpmnExclusiveGatewayName => ElementType.ExclusiveGateway,
            Constants.BpmnParallelGatewayName => ElementType.ParallelGateway,
            Constants.BpmnSendTaskName => ElementType.SendTask,
            Constants.BpmnReceiveTaskName => ElementType.ReceiveTask,
            Constants.BpmnServiceTaskName => ElementType.ServiceTask,
            Constants.BpmnSubProcess => ElementType.SubProcess,
            Constants.TextAnnotation => ElementType.TextAnnotation,
            Constants.Association => ElementType.Association,
            _ => throw new ArgumentOutOfRangeException($"[GetTypeNode] fail get: {shapeName}"),
        };
    }

    private IBpmnShape[] GetShapesFromPlane(XmlNode bpmnPlane, string idBpmnPlane)
    {
        var elements = new List<IBpmnShape>();

        foreach (XmlNode xmlNode in bpmnPlane.ChildNodes)
        {
            var element = xmlNode.Name switch
            {
                Constants.BpmnShapeName => CreateBpmnShape(xmlNode),
                Constants.BpmnEdgeName => CreateBpmnEdgeName(xmlNode),

                _ => throw new ArgumentOutOfRangeException($"{idBpmnPlane} {xmlNode.Name}"),
            };
            elements.Add(element);
        }

        if (elements.Any() is false)
        {
            throw new InvalidOperationException(
                $"[GetShapesFromPlane] Not find BpmnShape:{idBpmnPlane} {bpmnPlane.Name}");
        }

        return elements.ToArray();
    }

    private IBpmnShape CreateBpmnEdgeName(XmlNode xmlNode)
    {
        var id = GetId(xmlNode);
        var bpmnElement = GetBpmnElement(xmlNode);
        var waypoints = GetWaypoints(xmlNode);
        var labelBounds = GetBpmnLabelBound(xmlNode);

        return new BpmnEdge
        {
            Id = id,
            BpmnElement = bpmnElement,
            Waypoints = waypoints,
            BpmnLabel = labelBounds,
        };
    }

    private Waypoint[] GetWaypoints(XmlNode node)
    {
        var waypoints = node.ChildNodes
            .Cast<XmlNode>()
            .Where(p => p.Name == Constants.BpmnWaypointName)
            .Select(p =>
            {
                var x = p.Attributes?[Constants.BpmnXName]?.Value;
                var y = p.Attributes?[Constants.BpmnYName]?.Value;
                return new Waypoint
                {
                    X = Mappers.Map(x),
                    Y = Mappers.Map(y),
                };
            }).ToArray();

        if (waypoints.Any() is false)
        {
            throw new InvalidDataException($"Not Find waypoints from:{node.Name}");
        }

        return waypoints;
    }

    private IBpmnShape CreateBpmnShape(XmlNode xmlNode)
    {
        var id = GetId(xmlNode);
        var bpmnElement = GetBpmnElement(xmlNode);

        var bound = GetBounds(xmlNode);
        var labelBounds = GetBpmnLabelBound(xmlNode);

        return new BpmnShape
        {
            Id = id,
            Bounds = bound,
            BpmnElement = bpmnElement,
            BpmnLabel = labelBounds,
            TypeBpmnShape = BpmnShapeType.BpmnShape,
        };
    }

    private Bound GetBpmnLabelBound(XmlNode node)
    {
        var bounds = node.ChildNodes
            .Cast<XmlNode>()
            .Where(p => p.Name == Constants.BpmnLabelName)
            .SelectMany(p => p.ChildNodes.Cast<XmlNode>())
            .Select(p =>
            {
                var x = p.Attributes?[Constants.BpmnXName]?.Value;
                var y = p.Attributes?[Constants.BpmnYName]?.Value;
                var width = p.Attributes?[Constants.BpmnWidthName]?.Value;
                var height = p.Attributes?[Constants.BpmnHeightName]?.Value;
                return new Bound
                {
                    X = Mappers.Map(x),
                    Y = Mappers.Map(y),
                    Width = Mappers.Map(width),
                    Height = Mappers.Map(height),
                };
            }).ToArray();

        return bounds.FirstOrDefault() ?? new Bound();
    }

    private Bound GetBounds(XmlNode node)
    {
        var bound = node.ChildNodes
            .Cast<XmlNode>()
            .Where(p => p.Name == Constants.BpmnBoundsName)
            .Select(p =>
            {
                var x = p.Attributes?[Constants.BpmnXName]?.Value;
                var y = p.Attributes?[Constants.BpmnYName]?.Value;
                var width = p.Attributes?[Constants.BpmnWidthName]?.Value;
                var height = p.Attributes?[Constants.BpmnHeightName]?.Value;
                return new Bound
                {
                    X = Mappers.Map(x),
                    Y = Mappers.Map(y),
                    Width = Mappers.Map(width),
                    Height = Mappers.Map(height),
                };
            })
            .FirstOrDefault();

        if (bound is null)
        {
            throw new InvalidDataException($"Not Find GetBounds from:{node.Name}");
        }

        return bound;
    }

    private string GetBpmnElement(XmlNode node)
    {
        var bpmnElement = node.Attributes?[Constants.BpmnElementName]?.Value
                          ?? throw new InvalidOperationException($"Not Find BpmnElement from:{node.Name}");
        if (string.IsNullOrWhiteSpace(bpmnElement))
        {
            throw new InvalidDataException($"Not Find BpmnElement from:{node.Name}");
        }

        return bpmnElement;
    }

    private string GetId(XmlNode elements)
    {
        var id = elements.Attributes?[Constants.BpmnIdName]?.Value
                 ?? throw new InvalidOperationException($"Not Find ID from:{elements.Name}");
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidDataException($"Not Find ID from:{elements.Name}");
        }

        return id;
    }
}