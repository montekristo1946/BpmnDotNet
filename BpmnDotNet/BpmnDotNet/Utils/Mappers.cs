using System.Globalization;

namespace BpmnDotNet.Utils;

public static class Mappers
{
    public static int Map(string? x)
    {
        ArgumentNullException.ThrowIfNull(x);

        var res = float.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out var y);
        if (!res) throw new ArgumentException($"[Mappers] Could not convert {x} to {y}");

        return (int)y;
    }
}