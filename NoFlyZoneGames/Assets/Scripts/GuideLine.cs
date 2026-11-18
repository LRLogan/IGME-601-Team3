using System.Net;
using UnityEngine;

/// <summary>
/// Simple guild line class that is able to be expanded into something more complex if needed
/// </summary>
public sealed class GuideLine
{
    public Vector3 start, end;
    public Color color;
    public float width = 0.1f;
    public LineRenderer renderer;

    public GuideLine(Vector3 start, Vector3 end, Color color, float width, LineRenderer rend)
    {
        this.start = start;
        this.end = end;
        this.color = color;
        this.width = width;
        renderer = rend;
    }

    public GuideLine(Vector3 start, Vector3 end, Color color, LineRenderer rend)
    {
        this.start = start;
        this.end = end;
        this.color = color;
        renderer = rend;
    }

    /// <summary>
    /// Renders a line from point a to point b
    /// </summary>
    /// <param name="line"></param>
    public void EnableGuideLine(GuideLine line)
    {
        line.renderer.positionCount = 2;
        line.renderer.startWidth = line.width;
        line.renderer.endWidth = line.width;
        line.renderer.SetPosition(0, line.start);
        line.renderer.SetPosition(1, line.end);
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
