using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
//[RequireComponent(typeof(LineRenderer))]
public class BezierCubic : MonoBehaviour
{
    #region Class, data
    private List<Vector3> controlPoints = new List<Vector3>();
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
    #endregion

    #region MonoBehaviour
    #endregion

    #region Utilities
    public Vector3 this[int i]
    {
        get { return (Vector3)(transform.localToWorldMatrix * ControlPoints[i]) + transform.position; }
    }
    public void ResetToEmpty()
    {
        ControlPoints.Clear();
        ControlPoints = new List<Vector3>();
    }
    public void ResetToTemplate(Vector3 center)
    {
        if (ControlPoints == null)
            ControlPoints = new List<Vector3>();
        else
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
    /// Returns the control points of the given segment index. Local space. 
    /// </summary>
    /// <param name="i">Segment index. </param>
    public List<Vector3> SegmentPointsLocalSpace(int i)
    {
        return new List<Vector3>
        {
            controlPoints[i * 3],
            controlPoints[i * 3 + 1],
            controlPoints[i * 3 + 2],
            controlPoints[i * 3 + 3]
        };
    }
    /// <summary>
    /// Returns the control points of the given segment index. World space. 
    /// </summary>
    /// <param name="i">Segment index. </param>
    public List<Vector3> SegmentPointsWorldSpace(int i)
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
        List<Vector3> p = SegmentPointsLocalSpace(segment);
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
        List<Vector3> p = SegmentPointsLocalSpace(segment);
        t = Mathf.Clamp01(t);
        float omt = 1f - t;

        return
            3f * omt * omt * (p[1] - p[0])
            + 6f * omt * t * (p[2] - p[1])
            + 3f * t * t * (p[3] - p[2]);
    }
    public Vector3 Derivative2(int segment, float t)
    {
        List<Vector3> p = SegmentPointsLocalSpace(segment);
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
        Vector3 secondTangent = anchorPos + (ControlPoints[CountPoints - 1] - firstTangent);

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
    [Header("Constraints")]
    [SerializeField, Tooltip("Force all tangents to be directionally continuous. ")]
    public bool s_constraintContinuousDirections;
    [SerializeField, Tooltip("Force all tangents to be both directionally and magnitudally continuous. ")]
    public bool s_constraintContinuousTangents;
    [Header("Styling")]
    [Header("Styling: Colors")]
    [SerializeField, Tooltip("Default curve color. ")]
    public Color s_colorCurveDefault = new Color(235 / 255f, 235 / 255f, 235 / 255f, 1);
    [SerializeField, Tooltip("Hover curve color. ")]
    public Color s_colorCurveHover = new Color(255 / 255f, 255 / 255f, 255 / 255f, 1);
    [SerializeField, Tooltip("Clicked curve color. ")]
    public Color s_colorCurveClick = new Color(195 / 255f, 195 / 255f, 195 / 255f, 1);
    [SerializeField, Tooltip("Disabled curve color. ")]
    public Color s_colorCurveDisabled = new Color(155 / 255f, 155 / 255f, 155 / 255f, 1);
    [Space(5)]
    [SerializeField, Tooltip("Default tangent color. ")]
    public Color s_colorTangentDefault = new Color(255 / 255f, 55 / 255f, 55 / 255f, 1);
    [SerializeField, Tooltip("Hover tangent color. ")]
    public Color s_colorTangentHover = new Color(255 / 255f, 75 / 255f, 75 / 255f, 1);
    [SerializeField, Tooltip("Clicked tangent color. ")]
    public Color s_colorTangentClick = new Color(255 / 255f, 15 / 255f, 15 / 255f, 1);
    [SerializeField, Tooltip("Disabled tangent color. ")]
    public Color s_colorTangentDisabled = new Color(175 / 255f, 155 / 255f, 155 / 255f, 1);
    [Space(5)]
    [SerializeField, Tooltip("Default normal color. ")]
    public Color s_colorNormalDefault = new Color(55 / 255f, 255 / 255f, 55 / 255f, 1);
    [SerializeField, Tooltip("Hover normal color. ")]
    public Color s_colorNormalHover = new Color(75 / 255f, 255 / 255f, 75 / 255f, 1);
    [SerializeField, Tooltip("Clicked normal color. ")]
    public Color s_colorNormalClick = new Color(15 / 255f, 255 / 255f, 15 / 255f, 1);
    [SerializeField, Tooltip("Disabled normal color. ")]
    public Color s_colorNormalDisabled = new Color(155 / 255f, 175 / 255f, 155 / 255f, 1);
    [Space(5)]
    [SerializeField, Tooltip("Default bitangent color. ")]
    public Color s_colorBitangentDefault = new Color(55 / 255f, 55 / 255f, 255 / 255f, 1);
    [SerializeField, Tooltip("Hover bitangent color. ")]
    public Color s_colorBitangentHover = new Color(75 / 255f, 75 / 255f, 255 / 255f, 1);
    [SerializeField, Tooltip("Clicked bitangent color. ")]
    public Color s_colorBitangentClick = new Color(15 / 255f, 15 / 255f, 255 / 255f, 1);
    [SerializeField, Tooltip("Disabled bitangent color. ")]
    public Color s_colorBitangentDisabled = new Color(155 / 255f, 155 / 255f, 175 / 255f, 1);
    [Space(5)]
    [SerializeField, Tooltip("Default anchor point color. ")]
    public Color s_colorPointAnchorDefault = new Color(235 / 255f, 235 / 255f, 235 / 255f, 1);
    [SerializeField, Tooltip("Hover anchor point color. ")]
    public Color s_colorPointAnchorHover = new Color(255 / 255f, 255 / 255f, 255 / 255f, 1);
    [SerializeField, Tooltip("Clicked anchor point color. ")]
    public Color s_colorPointAnchorClick = new Color(195 / 255f, 195 / 255f, 195 / 255f, 1);
    [SerializeField, Tooltip("Disabled anchor point color. ")]
    public Color s_colorPointAnchorDisabled = new Color(155 / 255f, 155 / 255f, 155 / 255f, 1);
    [Space(5)]
    [SerializeField, Tooltip("Default discontinuous anchor point color. ")]
    public Color s_colorPointAnchorDiscontinuousDefault = new Color(195 / 255f, 195 / 255f, 195 / 255f, 1);
    [SerializeField, Tooltip("Hover discontinuous anchor point color. ")]
    public Color s_colorPointAnchorDiscontinuousHover = new Color(215 / 255f, 215 / 255f, 215 / 255f, 1);
    [SerializeField, Tooltip("Clicked discontinuous anchor point color. ")]
    public Color s_colorPointAnchorDiscontinuousClick = new Color(155 / 255f, 155 / 255f, 155 / 255f, 1);
    [SerializeField, Tooltip("Disabled discontinuous anchor point color. ")]
    public Color s_colorPointAnchorDiscontinuousDisabled = new Color(115 / 255f, 15 / 255f, 115 / 255f, 1);
    [Space(5)]
    [SerializeField, Tooltip("Default handle point color. ")]
    public Color s_colorPointHandleDefault = new Color(55 / 255f, 195 / 255f, 55 / 255f, 1);
    [SerializeField, Tooltip("Hover handle point color. ")]
    public Color s_colorPointHandleHover = new Color(55 / 255f, 215 / 255f, 55 / 255f, 1);
    [SerializeField, Tooltip("Clicked handle point color. ")]
    public Color s_colorPointHandleClick = new Color(55 / 255f, 175 / 255f, 55 / 255f, 1);
    [SerializeField, Tooltip("Disabled handle point color. ")]
    public Color s_colorPointHandleDisabled = new Color(155 / 255f, 175 / 255f, 155 / 255f, 1);
    [Space(5)]
    [SerializeField, Tooltip("Control polygon color. ")]
    public Color s_colorControlPolygon = new Color(55 / 255f, 55 / 255f, 55 / 255f, 1);
    [SerializeField, Tooltip("Control polygon's convex hull color. ")]
    public Color s_colorControlHull = new Color(55 / 255f, 55 / 255f, 55 / 255f, 0.1f);
    [Space(5)]
    [Header("Styling: Display Metrics")]
    [SerializeField, Tooltip("Segment resolution for LineRenderer. ")]
    public int s_curveResolution = 100;
    [SerializeField, Tooltip("Enforce uniform spherical handle radius?")]
    public bool s_handlePointRadiusFixed = false;
    [SerializeField, Tooltip("Uniform control point radius. ")]
    public float s_handlePointUniformRadius = 0.25f;
    [SerializeField, Tooltip("Radius of anchor point handles. ")]
    public float s_handlePointAnchorRadius = 0.25f;
    [SerializeField, Tooltip("Radius of handle point handles. ")]
    public float s_handlePointHandleRadius = 0.25f;
    [SerializeField, Tooltip("Radius of discontinuous anchor point handles. ")]
    public float s_handlePointAnchorDiscontinuousRadius = 0.15f;
    [Space(10)]
    [SerializeField, Tooltip("Enforce uniform width on curves? ")]
    public bool s_displayWidthFixed = false;
    [SerializeField, Tooltip("Uniform width of displayed curves, tangents, normals, and bitangents. ")]
    public float s_displayWidthUniform = 0.1f;
    [SerializeField, Tooltip("Width of displayed curve. ")]
    public float s_displayWidthCurve = 0.1f;
    [SerializeField, Tooltip("Width of displayed tangents. ")]
    public float s_displayWidthTangent = 0.05f;
    [SerializeField, Tooltip("Width of displayed normals. ")]
    public float s_displayWidthNormal = 0.05f;
    [SerializeField, Tooltip("Width of displayed bitangents. ")]
    public float s_displayWidthBitangent = 0.05f;
    [Space(5)]
    [SerializeField, Tooltip("Displays tangents and normals with fixed values (set below). ")]
    public bool s_displayLengthFixed = false;
    [SerializeField, Tooltip("Uniform length of displayed tangents, normals, and bitangents. ")]
    public float s_displayLengthUniform = 1f;
    [SerializeField, Tooltip("Length of displayed tangents (does not affect analytical values). ")]
    public float s_displayLengthTangent = 1f;
    [SerializeField, Tooltip("Length of displayed normals (does not affect analytical values). ")]
    public float s_displayLengthNormal = 1f;
    [SerializeField, Tooltip("Length of displayed bitangents (does not affect analytical values). ")]
    public float s_displayLengthBitangent = 1f;
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
