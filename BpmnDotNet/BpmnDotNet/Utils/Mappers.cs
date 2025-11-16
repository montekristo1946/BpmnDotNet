namespace BpmnDotNet.Utils;

using System.Globalization;

/// <summary>
/// Mappers.
/// </summary>
internal static class Mappers
{
    /// <summary>
    /// Map string to int.
    /// </summary>
    /// <param name="x">value in.</param>
    /// <returns>Int.</returns>
    public static int Map(string? x)
    {
        ArgumentNullException.ThrowIfNull(x);

        var res = float.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out var y);
        if (!res)
        {
            throw new ArgumentException($"[Mappers] Could not convert {x} to {y}");
        }

        return (int)y;
    }
}