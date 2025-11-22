namespace BpmnDotNet.Arm.Core.Dto;

/// <summary>
/// Размер окна.
/// </summary>
public class SizeWindows
{
    /// <summary>
    /// Gets or sets позиция в браузере, координата Х.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    ///  Gets or sets позиция в браузере, координата Y.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets ширина окна.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets высота окна.
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Пустое значение?.
    /// </summary>
    /// <returns>Bool.</returns>
    public bool IsEmpty()
    {
        return Width <= 0 || Height <= 0;
    }
}