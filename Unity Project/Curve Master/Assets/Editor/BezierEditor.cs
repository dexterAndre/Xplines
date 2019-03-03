using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
    To do: 
    - Translation, rotation, and scale handles on control points
    - Show handles on select
    - Settings: 
        - Display control polygon
        - Display curvature
        - Dashed lines display
        - Display projected curve
        - Display convex hull
        - Display AABB
*/

[CustomEditor(typeof(Bezier))]
public class BezierEditor : Editor
{
    // Class
    private Bezier bezier;
    private bool debug = true;

    // Settings
    [Tooltip("How close the mouse cursor has to be in order to detect point on curve. ")]
    private float s_curveSelectionRadius = 0.02f;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Reset to Default"))
        {
            bezier.ResetToDefault(bezier.transform.position);
        }
    }

    private void OnEnable()
    {
        bezier = (Bezier)target;

        if (bezier.ControlPoints == null)
        {
            bezier.CreateDefaultCurve(bezier.transform.position);
        }

        bezier.stylingColorPointCurve = new Color(255f / 255, 55f / 255, 55f / 255, 255f / 255);
        bezier.stylingColorPointTangent = new Color(55f / 255, 255f / 255, 55f / 255, 255f / 255);
        bezier.stylingColorCurve = new Color(55f / 255, 55f / 255, 255f / 255, 255f / 255);
        bezier.stylingColorTangent = new Color(25f / 255, 55f / 155, 55f / 25, 255f / 255);
        bezier.stylingColorPolygonControlStroke = new Color(255f / 255, 55f / 255, 55f / 255, 255f / 255);
        bezier.stylingColorPolygonControlFill = new Color(255f / 255, 55f / 255, 55f / 255, 255f / 255);
    }

    private void OnSceneGUI()
    {
        ProcessInput();
        Draw();
    }

    private void ProcessInput()
    {
        Event e = Event.current;
        Vector3 mousePos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;

        // Holding shift key
        if (e.shift == true)
        {
            // Hold shift to display closest point on curve
            if (e.type != EventType.MouseDown)
            {

            }
            // Shift-left-click to add point
            else if (
                e.type == EventType.MouseDown 
                && e.button == 0)
            {
                if (OverlappingCurve() == false)
                {
                    if (debug)
                        Debug.Log("Add Segment");
                    Undo.RecordObject(bezier, "Add Segment");
                    bezier.AddSegment(mousePos);
                }
                else
                {
                    if (debug)
                        Debug.Log("Split Segment");
                    Undo.RecordObject(bezier, "Split Segment");
                    bezier.SplitSegment(mousePos);
                }
            }
        }
    }
    private bool OverlappingCurve()
    {
        return false;
    }

    #region Drawing
    /// <summary>
    /// Draws curve by iterating equally along the parameter. 
    /// Pros: Simplest drawing algorithm. Control over line count. 
    /// Cons: Lines not evenly distributed. Not smooth over heavy curvature. 
    /// </summary>
    /// <param name="segment">Which segment to draw. </param>
    /// <param name="lines">How many lines in total to draw. </param>
    public void DrawByParameterIteration(int segment, int lines)
    {
        for (int j = 0; j <= lines - 1; j++)
        {
            Handles.DrawLine(
                bezier.EvaluateDeCasteljau(segment, (float)j / lines), 
                bezier.EvaluateDeCasteljau(segment, ((float)j + 1) / lines));
        }
    }
    /// <summary>
    /// Draws curve based on curvature. 
    /// Pros: Heavy curvature looks smoother. 
    /// Cons: No direct control over line count. Lines not evenly distributed. 
    /// </summary>
    /// <param name="segment">Which segment to draw. </param>
    /// <param name="threshold">Threshold in degrees on when to start a new line. </param>
    public void DrawByCurvature(int segment, float threshold)
    {

    }
    /// <summary>
    /// Draws curve by equal arc length distribution. 
    /// Pros: Equal lengths on every lines. 
    /// Cons: First and last line may not be of same length as other lines. Not smooth over heavy curvature. 
    /// </summary>
    /// <param name="segment">Which segment to draw. </param>
    /// <param name="length">Length of each arc. </param>
    /// <param name="dashed">Skip drawing every other line. Used for dashed visuals. </param>
    /// <param name="phase">Starting line offset. </param>
    public void DrawByArcLengthDistribution(int segment, float length, bool dashed, float phase)
    {

    }

    private void Draw()
    {
        // Drawing curve segments
        for (int i = 0; i < bezier.CountSegments; i++)
        {
            Vector3[] points = bezier.SegmentPoints(i);

            // Drawing control points
            Handles.color = bezier.stylingColorCurve;
            Handles.DrawLine(points[0], points[1]);
            Handles.DrawLine(points[2], points[3]);

            // Drawing curve
            Handles.DrawBezier(
                points[0], 
                points[3], 
                points[1], 
                points[2], 
                bezier.stylingColorPointCurve, 
                null, 
                bezier.stylingCurveWidth);
            DrawByParameterIteration(i, 100);
        }

        // Drawing handles
        Handles.color = bezier.stylingColorTangent;

        for (int i = 0; i < bezier.CountControlPoints; i++)
        {
            Vector3 newPos = Handles.FreeMoveHandle(
                bezier[i],
                Quaternion.identity,
                0.1f,
                Vector3.zero,
                Handles.CylinderHandleCap);

            if (bezier[i] != newPos)
            {
                Undo.RecordObject(bezier, "Translate Point");
                bezier.TranslatePoint(i, newPos);
            }
        }
    }
    #endregion

    #region Visualization
    // Closest point to mouse position
    #endregion
}
