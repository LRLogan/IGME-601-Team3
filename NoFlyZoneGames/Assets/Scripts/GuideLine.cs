using System.Net;
using UnityEngine;

/// <summary>
/// Simple guild line class that is able to be expanded into something more complex if needed
/// </summary>
public sealed class GuideLine
{
    public Vector3 start, end;
    public Color color;
    public float width = 0.2f;
    public LineRenderer renderer;

    public GuideLine(Vector3 start, Vector3 end, Color color, float width   )
    {
        this.start = start;
        this.end = end;
        this.color = color;
        this.width = width;
    }

    public GuideLine(Vector3 start, Vector3 end, Color color)
    {
        this.start = start;
        this.end = end;
        this.color = color;
    }

    /// <summary>
    /// Renders a line from point a to point b
    /// </summary>
    /// <param name="line"></param>
    public void EnableGuideLine(GuideLine line)
    {
        LineRenderer newLine = new LineRenderer();

        newLine.positionCount = 2;
        newLine.startWidth = width;
        newLine.endWidth = width;

        newLine.SetPosition(0, line.start);
        newLine.SetPosition(1, line.end);

        line.renderer = newLine;
    }

    /// <summary>
    /// Clears the line renderer
    /// </summary>
    /// <param name="line"></param>
    public void DisableGuideLine(GuideLine line)
    {
        line.renderer.enabled = false;
        line.renderer = null;
    }
}
