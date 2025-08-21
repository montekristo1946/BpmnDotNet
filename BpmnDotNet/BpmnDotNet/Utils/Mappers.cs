namespace BpmnDotNet.Utils;

public static class Mappers
{
    public static int Map(string? x)
    {
        ArgumentNullException.ThrowIfNull(x);
        
        var res = Int32.TryParse(x, out Int32 y);
        if (!res)
        {
            throw new ArgumentException($"[Mappers] Could not convert {x} to {y}");
        }

        return y;
    }
}