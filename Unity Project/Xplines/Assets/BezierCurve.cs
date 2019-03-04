using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
[RequireComponent(typeof(LineRenderer))]
public class BezierCurve : MonoBehaviour
{
    #region Class, data
    private List<Vector3> controlPoints;
    public List<Vector3> ControlPoints
    {
        get { return controlPoints; }
        set { controlPoints = value; }
    }
    public BezierCurve(Vector3 center)
    {
        GenerateDefaultCurve(center);
    }
    public Vector3 this[int i]
    {
        get { return ControlPoints[i]; }
    }
    #endregion

    #region Utilities
    private void GenerateDefaultCurve(Vector3 center)
    {
        ControlPoints.Clear();
        ControlPoints = new List<Vector3>
        {
            center + Vector3.left,
            center + (Vector3.left + Vector3.up) * 0.5f,
            center + (Vector3.right + Vector3.down) * 0.5f,
            center + Vector3.right
        };
    }
    private List<Vector3> SegmentPoints(int i)
    {
        return new List<Vector3>
        {
            this[i * 3],
            this[i * 3 + 1],
            this[i * 3 + 2],
            this[i * 3 + 3]
        };
    }
    private int CountSegments
    {
        get { return (ControlPoints.Count - 4) / 3 + 1; }
    }
    private int CountPoints
    {
        get { return ControlPoints.Count; }
    }
    #endregion

    #region Analysis
    public Vector3 Evaluate(int segment, float t)
    {
        List<Vector3> p = SegmentPoints(segment);
        t = Mathf.Clamp01(t);
        float omt = 1f - t;

        return
            omt * omt * omt * p[0]
            + 3f * omt * omt * t * p[1]
            + 3f * omt * t * t * p[2]
            + t * t * t * p[3];
    }
    public Vector3 Derivative(int segment, float t)
    {
        List<Vector3> p = SegmentPoints(segment);
        t = Mathf.Clamp01(t);
        float omt = 1f - t;

        return
            3f * omt * omt * (p[1] - p[0])
            + 6f * omt * t * (p[2] - p[1])
            + 3f * t * t * (p[3] - p[2]);
    }
    public Vector3 Derivative2(int segment, float t)
    {
        List<Vector3> p = SegmentPoints(segment);
        t = Mathf.Clamp01(t);
        float omt = 1f - t;
        float om2t = 1f - 2 * t;

        return 6f * (
            t * (p[3] - p[2])
            + om2t * (p[2] - p[1])
            - omt * (p[1] - p[0]));
    }
    public Vector3 Tangent(int segment, float t)
    {
        return Derivative(segment, t);
    }
    public Vector3 NormalFrenet(int segment, float t)
    {
        Vector3 a = Tangent(segment, t).normalized;
        Vector3 b = (a + Derivative2(segment, t)).normalized;
        Vector3 r = Vector3.Cross(b, a);
        return Vector3.Cross(r, a);
    }
    #endregion

    #region Editing
    #endregion

    #region Editor settings
    #endregion

    #region Intersection
    #endregion

    #region Curve fitting
    #endregion

    #region Curve events
    #endregion

    #region Mesh generation
    #endregion

    #region Curve physics
    #endregion
}
