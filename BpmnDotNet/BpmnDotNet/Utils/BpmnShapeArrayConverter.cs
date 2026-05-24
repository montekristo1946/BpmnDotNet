namespace BpmnDotNet.Utils;

using System.Text.Json;
using System.Text.Json.Serialization;
using BpmnDotNet.BPMNDiagram;
using BpmnDotNet.BPMNDiagram.Abstractions;
using static System.Text.Json.JsonSerializer;

/// <summary>
/// Json convert Utf8JsonReader in IBpmnShape[].
/// </summary>
internal class BpmnShapeArrayConverter : JsonConverter<IBpmnShape[]>
{
    /// <inheritdoc/>
    public override IBpmnShape[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        var shapes = new List<IBpmnShape>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            var typeBpmnShapeStr = root.GetProperty("TypeBpmnShape").GetString();
            if (typeBpmnShapeStr is null)
            {
                throw new JsonException("[IBpmnShapeArrayConverter:Read] Failed to parse TypeBpmnShape");
            }

            var resParse = Enum.TryParse<BpmnShapeType>(typeBpmnShapeStr, out BpmnShapeType typeBpmnShape);
            if (!resParse)
            {
                throw new JsonException($"[IBpmnShapeArrayConverter:Read] Failed convert {typeBpmnShapeStr} to enum TypeBpmnShape");
            }

#pragma warning disable IL2026
            IBpmnShape? shape = typeBpmnShape switch
            {
                BpmnShapeType.BpmnEdge => Deserialize<BpmnEdge>(root.GetRawText(), options),
                BpmnShapeType.BpmnShape => Deserialize<BpmnShape>(root.GetRawText(), options),
                _ => throw new NotSupportedException($"Unknown shape type: {typeBpmnShape}"),
            };
#pragma warning restore IL2026

            if (shape is null)
            {
                throw new JsonException("[IBpmnShapeArrayConverter:Read] Failed deserialize BpmnShape or BpmnEdge");
            }

            shapes.Add(shape);
        }

        return shapes.ToArray();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IBpmnShape[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var shape in value)
        {
#pragma warning disable IL2026
            Serialize(writer, shape, shape.GetType(), options);
#pragma warning restore IL2026
        }

        writer.WriteEndArray();
    }
}