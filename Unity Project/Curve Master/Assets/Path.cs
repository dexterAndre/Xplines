using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    [SerializeField, HideInInspector]
    private List<Vector2> _points;

    // Default constructor
    public Path(Vector2 center)
    {
        _points = new List<Vector2>
        {
            center + Vector2.left,
            center + (Vector2.left + Vector2.up) * 0.5f,
            center + (Vector2.right + Vector2.down) * 0.5f,
            center + Vector2.right
        };
    }

    #region Curve Editing
    public void AddSegment(Vector2 anchorPos)
    {
        // Calculating intermetiate control points
        Vector2 nextPos = 2 * _points[_points.Count - 1] - _points[_points.Count - 2];
        Vector2 middlePos = nextPos + (anchorPos - nextPos) * 0.5f;

        // Adding points to list
        _points.Add(nextPos);
        _points.Add(middlePos);
        _points.Add(anchorPos);
    }
    public void RepositionPoint(int i, Vector2 pos)
    {
        // Placeholder functionality. Will improve later on. 
        _points[i] = pos;
    }
    #endregion

    #region Utilities
    // Accesses points at index i
    public Vector2 this[int i]
    {
        get
        {
            return _points[i];
        }
    }

    // Property for counting total amount of points in curve
    public int CountPoints
    {
        get
        {
            return _points.Count;
        }
    }

    // Property for counting total amount of curve segments in curve
    public int CountSegments
    {
        get
        {
            return (_points.Count - 4) / 3 + 1;
        }
    }

    // Accesses the points of a given segment
    public Vector2[] GetSegmentPoints(int i)
    {
        return new Vector2[]
        {
            _points[i * 3],
            _points[i * 3 + 1],
            _points[i * 3 + 2],
            _points[i * 3 + 3]
        };
    }
    #endregion
}
