namespace BpmnDotNet.Arm.Core.Dto;

public class SizeWindows
{
    public double X { get; set; }
    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public bool IsEmpty()
    {
        return Width <= 0 || Height <= 0;
    }
}