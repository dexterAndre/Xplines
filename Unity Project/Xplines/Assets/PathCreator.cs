using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public Path _path;

    [Header("Styling")]
    public Color _colorHandlePoint;
    public Color _colorHandleTangent;
    public Color _colorHandleLine;
    public Color _colorPolygon;
    public Color _colorCurve;

    public void CreatePath()
    {
        _path = new Path(transform.position);
    }
}
