namespace BpmnDotNet.Utils;

public static class Mappers
{
    public static int Map(string? x)
    {
        ArgumentNullException.ThrowIfNull(x);

        var res = int.TryParse(x, out var y);
        if (!res) throw new ArgumentException($"[Mappers] Could not convert {x} to {y}");

        return y;
    }
}