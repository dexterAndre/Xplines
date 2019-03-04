using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Resources: 
    - Sebastian Lague: [Unity] Curve Editor https://youtu.be/RF04Fi9OCPc 
    - Pomax: A Primer on Bézier Curves https://pomax.github.io/bezierinfo/ 
    - CatlikeCoding: Curves and Splines https://catlikecoding.com/unity/tutorials/curves-and-splines/
    - Continuity types: https://www.sharcnet.ca/Software/Gambit/html/faq/geometry_check.htm 
    - More on continuity: http://graphics.stanford.edu/courses/cs348a-17-winter/ReaderNotes/handout27.pdf 
    - Continuity guide: https://www.algosome.com/articles/continuous-bezier-curve-line.html 
    - Even more on cintunuity: http://www.cs.uky.edu/~cheng/cs535/Notes/CS535-Curves-1.pdf
*/

[System.Serializable]
[RequireComponent(typeof(LineRenderer))]
public class BezierCubic : MonoBehaviour
{
    #region Class, data
    private List<Vector3> controlPoints;
    public List<Vector3> ControlPoints
    {
        get
        {
            return controlPoints;
        }
        set
        {
            controlPoints = value;

            // Reset processes such as events, etc. 
            // Reset look-up tables such as RM frames, arc length, etc. 
        }
    }
    public BezierCubic(Vector3 center)
    {
        ResetToTemplate(center);
    }
    public Vector3 this[int i]
    {
        get { return ControlPoints[i]; }
    }
    #endregion

    #region Utilities
    public void ResetToEmpty()
    {
        ControlPoints.Clear();
    }
    public void ResetToTemplate(Vector3 center)
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
    /// <summary>
    /// Returns the control points of the given segment index. 
    /// </summary>
    /// <param name="i">Segment index. </param>
    public List<Vector3> SegmentPoints(int i)
    {
        return new List<Vector3>
        {
            this[i * 3],
            this[i * 3 + 1],
            this[i * 3 + 2],
            this[i * 3 + 3]
        };
    }
    /// <summary>
    /// Counts the amount of control points. 
    /// Includes both anchor points and tangent handles. 
    /// </summary>
    public int CountPoints
    {
        get { return ControlPoints.Count; }
    }
    /// <summary>
    /// Counts the amount of Bézier curve segments. 
    /// </summary>
    public int CountSegments
    {
        get { return (ControlPoints.Count - 4) / 3 + 1; }
    }
    /// <summary>
    /// Tests if the control point index yields an anchor point. 
    /// True if anchor point. False if tangent handle. 
    /// </summary>
    /// <param name="i">Control point index. </param>
    public bool IsAnchor(int i)
    {
        return i % 3 == 0 ? true : false;
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
    public Vector3 NormalRotatonalMinimizing(int segment, float t)
    {
        // Incomplete
        return Vector3.negativeInfinity;
    }
    #endregion

    #region Editing
    /// <summary>
    /// Appends a Bézier segment after last segment. 
    /// Automatically calculates intermediate tangents. 
    /// </summary>
    /// <param name="anchorPos">Position of new anchor point. </param>
    public void AddSegment(Vector3 anchorPos)
    {
        // Intermediate control points
        Vector3 firstTangent = 2 * ControlPoints[CountPoints - 1] - ControlPoints[CountPoints - 2];
        Vector3 secondTangent = firstTangent + (anchorPos - firstTangent) * 0.5f;

        // Adding points to list
        ControlPoints.Add(firstTangent);
        ControlPoints.Add(secondTangent);
        ControlPoints.Add(anchorPos);
    }
    /// <summary>
    /// Appends a Bézier segment after last segment. 
    /// Automatically calculates first tangent, but uses tangentPos for the last tangent handle. 
    /// </summary>
    /// <param name="anchorPos">Position of new anchor point. </param>
    /// <param name="tangentPos">Position of last tangent handle. </param>
    public void AddSegment(Vector3 anchorPos, Vector3 tangentPos)
    {

    }
    /// <summary>
    /// Translates an anchor point. Also translates its corresponding tangents without breaking continuity. 
    /// </summary>
    /// <param name="i">Control point index. </param>
    /// <param name="pos">New position. </param>
    public void TranslatePoint(int i, Vector3 pos)
    {
        // Incomplete
        ControlPoints[i] = pos;

        // Also translate tangents
    }
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
