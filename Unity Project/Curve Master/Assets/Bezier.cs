using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    To do: 
*/

[System.Serializable]
public class Bezier : MonoBehaviour
{
    #region Editor
    public float tangentLength = 0.2f;
    public float normalLength = 0.2f;
    #endregion

    #region Class
    [SerializeField]
    private List<Vector3> controlPoints;
    public List<Vector3> ControlPoints
    {
        get { return controlPoints; }
    }

    public Bezier(Vector3 center)
    {
        CreateDefaultCurve(center);
    }
    public void CreateDefaultCurve(Vector3 center)
    {
        controlPoints = new List<Vector3>
        {
            center + Vector3.left,
            center + (Vector3.left + Vector3.up) * 0.5f,
            center + (Vector3.right + Vector3.down) * 0.5f,
            center + Vector3.right
        };
    }
    public void ResetToDefault(Vector3 center)
    {
        controlPoints.Clear();
        CreateDefaultCurve(center);
    }
    /// <summary>
    /// Gets second iteration of points. 
    /// </summary>
    /// <param name="segment">Which segment to operate on. </param>
    /// <param name="t">Curve parameter. </param>
    /// <returns></returns>
    private Vector3[] GetQ(int segment, float t)
    {
        Vector3[] P = SegmentPoints(segment);
        Vector3[] dP = { P[1] - P[0], P[2] - P[1], P[3] - P[2] };
        return new Vector3[] { P[0] + t * dP[0], P[1] + t * dP[1], P[2] + t * dP[2] };
    }
    /// <summary>
    /// Gets third iteration of points. 
    /// </summary>
    /// <param name="segment">Which segment to operate on. </param>
    /// <param name="t">Curve parameter. </param>
    /// <returns></returns>
    private Vector3[] GetR(int segment, float t)
    {
        Vector3[] Q = GetQ(segment, t);
        Vector3[] dQ = { Q[1] - Q[0], Q[2] - Q[1] };
        return new Vector3[] { Q[0] + t * dQ[0], Q[1] + t * dQ[1] };
    }
    public Vector3 EvaluateDeCasteljau(int segment, float t)
    {
        //Vector3[] P = SegmentPoints(segment);
        //Vector3[] dP = { P[1] - P[0], P[2] - P[1], P[3] - P[2] };
        //Vector3[] Q = { P[0] + t * dP[0], P[1] + t * dP[1], P[2] + t * dP[2] };
        //Vector3[] dQ = { Q[1] - Q[0], Q[2] - Q[1] };
        //Vector3[] R = { Q[0] + t * dQ[0], Q[1] + t * dQ[1] };
        Vector3[] R = GetR(segment, t);
        Vector3 S = R[0] + t * (R[1] - R[0]);

        return S;
    }
    public Vector3 EvaluateAnalytical(int segment, float t)
    {
        Vector3[] p = SegmentPoints(segment);

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
        // https://catlikecoding.com/unity/tutorials/curves-and-splines/
        Vector3[] p = SegmentPoints(segment);

        t = Mathf.Clamp01(t);
        float omt = 1f - t;

        return
            3f * omt * omt * (p[1] - p[0])
            + 6f * omt * t * (p[2] - p[1])
            + 3f * t * t * (p[3] - p[2]);
    }
    public Vector3 Derivative2(int segment, float t)
    {
        Vector3[] p = SegmentPoints(segment);

        t = Mathf.Clamp01(t);
        float omt = 1f - t;
        float om2t = 1f - 2 * t;

        return
            -6f * omt * (p[1] - p[0])
            + 6f * om2t * (p[2] - p[1])
            + 6f * t * (p[3] - p[2]);
    }
    public Vector3 Tangent(int segment, float t)
    {
        return Derivative(segment, t);
    }
    public Vector3 Normal(int segment, float t)
    {
        Vector3 p = EvaluateAnalytical(segment, t);
        Vector3 bd = Derivative(segment, t);
        float mag = bd.magnitude;
        Vector3 bdd = Derivative2(segment, t);
        Vector3 rotAxis = Vector3.Cross(bd, bdd);
        return Vector3.Cross(bd, rotAxis);
    }
    #endregion

    #region Styling
    [Header("Styling"), Space(5)]
    [Header("Curve")]
    [SerializeField]
    public Color stylingColorCurve = new Color(55, 55, 255, 255);
    [SerializeField]
    public float stylingCurveWidth = 2f;
    [Space(5), Header("Points")]
    [SerializeField]
    public Color stylingColorPointCurve = new Color(255, 55, 55, 255);
    [SerializeField]
    public Color stylingColorPointTangent = new Color(55, 255, 55, 255);
    [SerializeField]
    public Color stylingColorTangent = new Color(25, 155, 25, 255);
    [Space(5), Header("Hull")]
    [SerializeField]
    public Color stylingColorPolygonControlStroke;
    [SerializeField]
    public Color stylingColorPolygonControlFill;
    #endregion

    #region Editing
    public void AddSegment(Vector3 anchorPos)
    {
        // Intermediate control points
        Vector3 nextPos = 2 * controlPoints[controlPoints.Count - 1] - controlPoints[controlPoints.Count - 2];
        Vector3 middlePos = nextPos + (anchorPos - nextPos) * 0.5f;

        // Adding points to list
        controlPoints.Add(nextPos);
        controlPoints.Add(middlePos);
        controlPoints.Add(anchorPos);
    }
    public void SplitSegment(Vector3 anchorPos)
    {

    }
    public void TranslatePoint(int i, Vector3 pos)
    {
        // Temporary functionality. Will improve upon later. 
        controlPoints[i] = pos;
    }
    #endregion

    #region Analysis
    // Bitangent
    // Curvature
    // Arc length
    // Curve fitting
    // Computational complexity
    // Inflection points
    // Extrema? 
    // Real-time inspector editing
    // Inspector unit interval Bézier
    // Bézier to function
    #endregion

    #region Events
    // Event point
    // Event interval
    #endregion

    #region Utilities
    /// <summary>
    /// Accessor operator for accessing point number i. 
    /// </summary>
    /// <param name="i">Point index. </param>
    /// <returns></returns>
    public Vector3 this[int i]
    {
        get { return controlPoints[i]; }
    }
    /// <summary>
    /// Returns amount of control points in curve. 
    /// </summary>
    public int CountControlPoints
    {
        get { return controlPoints.Count; }
    }
    /// <summary>
    /// Returns amount of Bézier segments in curve. 
    /// </summary>
    public int CountSegments
    {
        get { return (controlPoints.Count - 4) / 3 + 1; }
    }
    /// <summary>
    /// Returns control points belonging to segment number i. 
    /// </summary>
    /// <param name="i">Segment index. </param>
    /// <returns></returns>
    public Vector3[] SegmentPoints(int i)
    {
        return new Vector3[]
        {
            controlPoints[i * 3],
            controlPoints[i * 3 + 1],
            controlPoints[i * 3 + 2],
            controlPoints[i * 3 + 3]
        };
    }
    #endregion

    #region MonoBehavior
    private void OnEnable()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        // Drawing Bézier curve
        for (int i = 0; i < CountSegments; i++)
        {
            // Draws Bézier curve
            DrawByParameterIteration(i, 100);

            // Draws tangents
        }

        // Drawing control points
        for (int i = 0; i < CountControlPoints; i++)
        {
            Gizmos.DrawSphere(ControlPoints[i], 0.05f);
        }
    }
    #endregion

    #region Drawing
    private void DrawByParameterIteration(int segment, int lines)
    {
        for (int j = 0; j <= lines - 1; j++)
        {
            //Gizmos.DrawLine(
            //    EvaluateDeCasteljau(segment, (float)j / lines),
            //    EvaluateDeCasteljau(segment, ((float)j + 1) / lines));

            Vector3 p = EvaluateAnalytical(segment, (float)j / lines);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                p,
                EvaluateAnalytical(segment, ((float)j + 1) / lines));

            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                Derivative(segment, (float)j / lines),
                Derivative(segment, ((float)j + 1) / lines));

            Gizmos.color = Color.blue;
            Vector3 tangent = Tangent(segment, (float)j / lines);
            Gizmos.DrawLine(p, p + tangent.normalized * tangentLength);

            Gizmos.color = Color.magenta;
            Vector3 normal = Normal(segment, (float)j / lines);
            Gizmos.DrawLine(p, p + normal.normalized * normalLength);
        }
    }
    #endregion
}
