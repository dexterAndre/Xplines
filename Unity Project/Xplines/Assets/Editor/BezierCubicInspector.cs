using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierCubic))]
public class BezierCubicInspector : Editor
{
    // Data
    private BezierCubic curve = null;
    private bool isEditing = false;
    private bool expandInstructions = false;
    private bool expandAdvanced = false;
    private bool expandDisplaySettings = false;
    private bool expandDisplaySettingsMetric = false;
    private bool expandDisplaySettingsColor = false;
    [System.Serializable]
    private enum AddMode
    {
        Collinear,
        CameraForward,
        XZ_Plane,
        Tangent
    };
    private AddMode addMode = AddMode.XZ_Plane;
    /// <summary>
    /// How far along selected AddMode to add new anchor point. 
    /// E.g. x along tangent, x in front of camera, etc. 
    /// </summary>
    private float addModeDistance = 5f;

    //private float s_curveWidth = 5f;
    //private Color s_curveColor = Color.magenta;
    private float s_pointRadius = 0.25f;
    private Color s_pointColorAnchor = Color.blue;
    private Color s_pointColorSelected = Color.white;
    private Color s_pointColorHandle = Color.gray;
    private float s_snap = 0.5f;

    private int selectedPoint = -1;

    private Vector2 clickPos;
    private Vector2 dragDelta;

    // Styling
    private static GUIStyle ToggleButtonStyleOn = null;
    private static GUIStyle ToggleButtonStyleOff = null;
    private void SetToggleButtonStyles()
    {
        // https://gamedev.stackexchange.com/questions/98920/how-do-i-create-a-toggle-button-in-unity-inspector
        if (ToggleButtonStyleOff == null)
        {
            ToggleButtonStyleOff = "Button";
        }
        if (ToggleButtonStyleOn == null)
        {
            ToggleButtonStyleOn = new GUIStyle(ToggleButtonStyleOff);
            ToggleButtonStyleOn.normal.background = ToggleButtonStyleOn.active.background;
        }
    }

    private void OnEnable()
    {
        curve = (BezierCubic)target;
        if (curve.ControlPoints == null)
        {
            curve.ResetToTemplate(Vector3.zero);
        }

        // Setting default settings
        SetDefaultDisplayMetrics();
        SetDefaultColors();
    }

    private void OnSceneGUI()
    {
        ProcessInput();
        DrawScene();
    }

    private void ProcessInput()
    {
        // Editing
        Event e = Event.current;
        Vector3 mousePos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;

        // Holding shift
        if (e.shift == true)
        {
            // Shift left-click
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                clickPos = Input.mousePosition;
                Debug.Log("Shift left-click");

                switch (addMode)
                {
                    case AddMode.Collinear:
                    {
                        break;
                    }
                    case AddMode.CameraForward:
                    {
                        Camera cam;
                        if (Camera.current != null)
                            cam = Camera.current;
                        else
                            cam = Camera.main;

                        Vector3 camPoint
                            = cam.transform.position
                            + cam.transform.forward
                            * addModeDistance;

                        curve.AddSegment(camPoint);
                        break;
                    }
                    case AddMode.XZ_Plane:
                    {
                        Camera cam;
                        if (Camera.current != null)
                            cam = Camera.current;
                        else
                            cam = Camera.main;

                        Ray ray = HandleUtility.GUIPointToWorldRay(new Vector3(e.mousePosition.x, e.mousePosition.y));
                        Vector3 dir = ray.direction;


                        // Handling cases that will never hit he xz-plane
                        if (cam.transform.position.y > 0f && dir.y >= 0f)
                        {
                            // Failed to hit xz-plane
                            break;
                        }
                        else if (cam.transform.position.y < 0f && dir.y <= 0f)
                        {
                            // Failed to hit xz-plane
                            break;
                        }

                        Vector3 pos = ray.origin;
                        float t = -pos.y / dir.y;

                        Vector3 xzPoint = pos + t * dir;

                        curve.AddSegment(xzPoint);
                        break;
                    }
                    case AddMode.Tangent:
                    {
                        break;
                    }
                }
            }
            // If scrolling
            if (e.isScrollWheel)
            {
                // Shift-scroll to adjust addModeCameraForwardDistance
                addModeDistance -= (float)e.delta.y / 10f;
                if (addModeDistance < 0f)
                    addModeDistance = 0f;
                // Incomplete - use settings to determing min / max addModeCameraForwardDistance!
                else if (addModeDistance > 10f)
                    addModeDistance = 10f;

                Debug.Log(addModeDistance);
            }
        }
    }

    private void DrawScene()
    {
        // Displaying curve
        if (curve.ControlPoints != null)
        {
            for (int i = 0; i < curve.CountSegments; i++)
            {
                List<Vector3> p = curve.SegmentPointsWorldSpace(i);
                Handles.DrawBezier(
                    p[0],
                    p[3],
                    p[1],
                    p[2],
                    curve.s_colorCurveDefault,
                    null,
                    curve.s_displayWidthFixed == true ? curve.s_displayWidthUniform : curve.s_displayWidthCurve);
            }

            // Displaying anchor points
            for (int i = 0; i < curve.CountPoints; i++)
            {
                // Check if this point is anchor
                if (curve.IsAnchor(i))
                {
                    Handles.color = s_pointColorAnchor;
                }
                // Else it is a tangent handle
                else
                {
                    // Draw tangent lines
                    //      Out-tangent
                    if (curve.IsAnchor(i - 1))
                    {
                        Handles.DrawLine(curve[i - 1], curve[i]);
                    }
                    //      In-tangent
                    else
                    {
                        Handles.DrawLine(curve[i + 1], curve[i]);
                    }

                    Handles.color = s_pointColorHandle;
                }

                // Selected point is special (overrides previously selected color)
                if (selectedPoint == i)
                {
                    Handles.color = s_pointColorSelected;
                }

                Vector3 pos = Handles.FreeMoveHandle(curve[i], Quaternion.identity, s_pointRadius, Vector3.zero, Handles.SphereHandleCap);
                if (curve[i] != pos)
                {
                    Undo.RecordObject(curve, "Translate Point");
                    curve.TranslatePoint(i, pos);
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        if (curve == null)
            curve = (BezierCubic)target;
        SetToggleButtonStyles();

        // Instructions foldout (may be redundant - consider removing based on user editing controls)
        if (GUILayout.Button("Edit Curve", isEditing == true ? ToggleButtonStyleOn : ToggleButtonStyleOff))
        {
            isEditing = !isEditing;
            Debug.Log("Clicked Edit Curve!");
        }
        addMode = (AddMode)EditorGUILayout.EnumPopup("Add Mode: ", addMode);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Undo"))
        {
            Debug.Log("Clicked Undo!");
        }
        if (GUILayout.Button("Redo"))
        {
            Debug.Log("Clicked Redo!");
        }
        GUILayout.EndHorizontal();

        #region Display Settings
        expandDisplaySettings = EditorGUILayout.Foldout(expandDisplaySettings, "Display Settings");
        if (expandDisplaySettings == true)
        {
            EditorGUI.indentLevel++;

            // Display metrics
            expandDisplaySettingsMetric = EditorGUILayout.Foldout(expandDisplaySettingsMetric, "Display Metrics");
            if (expandDisplaySettingsMetric == true)
            {
                EditorGUILayout.LabelField("Control Point Radii");
                curve.s_handlePointRadiusFixed = EditorGUILayout.Toggle("Enforce Uniform Radii", curve.s_handlePointRadiusFixed);
                curve.s_handlePointUniformRadius = EditorGUILayout.FloatField("Uniform Radius", curve.s_handlePointUniformRadius);
                EditorGUILayout.Space();
                curve.s_handlePointAnchorRadius = EditorGUILayout.FloatField("Anchor Point Radius", curve.s_handlePointAnchorRadius);
                curve.s_handlePointAnchorDiscontinuousRadius = EditorGUILayout.FloatField("Broken Anchor Point Radius", curve.s_handlePointAnchorDiscontinuousRadius);
                curve.s_handlePointHandleRadius = EditorGUILayout.FloatField("Handle Point Radius", curve.s_handlePointHandleRadius);
                EditorGUILayout.LabelField("Widths");
                curve.s_displayWidthFixed = EditorGUILayout.Toggle("Enforce Uniform Widths", curve.s_displayWidthFixed);
                curve.s_displayWidthUniform = EditorGUILayout.FloatField("Uniform Width", curve.s_displayWidthUniform);
                EditorGUILayout.Space();
                curve.s_displayWidthCurve = EditorGUILayout.FloatField("Curve Width", curve.s_displayWidthCurve);
                curve.s_displayWidthTangent = EditorGUILayout.FloatField("Tangent Width", curve.s_displayWidthTangent);
                curve.s_displayWidthNormal = EditorGUILayout.FloatField("Normal Width", curve.s_displayWidthNormal);
                curve.s_displayWidthBitangent = EditorGUILayout.FloatField("Bitangent Width", curve.s_displayWidthBitangent);
                EditorGUILayout.LabelField("Lengths");
                curve.s_displayLengthFixed = EditorGUILayout.Toggle("Enforce Uniform Lengths", curve.s_displayLengthFixed);
                curve.s_displayLengthUniform = EditorGUILayout.FloatField("Uniform Length", curve.s_displayLengthUniform);
                EditorGUILayout.Space();
                curve.s_displayLengthTangent = EditorGUILayout.FloatField("Tangent Length", curve.s_displayLengthTangent);
                curve.s_displayLengthNormal = EditorGUILayout.FloatField("Normal Length", curve.s_displayLengthNormal);
                curve.s_displayLengthBitangent = EditorGUILayout.FloatField("Bitangent Length", curve.s_displayLengthBitangent);
            }

            // Colors
            expandDisplaySettingsColor = EditorGUILayout.Foldout(expandDisplaySettingsColor, "Colors");
            if (expandDisplaySettingsColor == true)
            {
                EditorGUILayout.LabelField("Curve Colors");
                curve.s_colorCurveDefault = EditorGUILayout.ColorField("Curve Default", curve.s_colorCurveDefault);
                curve.s_colorCurveHover = EditorGUILayout.ColorField("Curve Hover", curve.s_colorCurveHover);
                curve.s_colorCurveClick = EditorGUILayout.ColorField("Curve Clicked", curve.s_colorCurveClick);
                curve.s_colorCurveDisabled = EditorGUILayout.ColorField("Curve Disabled", curve.s_colorCurveDisabled);
                EditorGUILayout.LabelField("Tangent Colors");
                curve.s_colorTangentDefault = EditorGUILayout.ColorField("Tangent Default", curve.s_colorTangentDefault);
                curve.s_colorTangentHover = EditorGUILayout.ColorField("Tangent Hover", curve.s_colorTangentHover);
                curve.s_colorTangentClick = EditorGUILayout.ColorField("Tangent Clicked", curve.s_colorTangentClick);
                curve.s_colorTangentDisabled = EditorGUILayout.ColorField("Tangent Disabled", curve.s_colorTangentDisabled);
                EditorGUILayout.LabelField("Normal Colors");
                curve.s_colorNormalDefault = EditorGUILayout.ColorField("Normal Default", curve.s_colorNormalDefault);
                curve.s_colorNormalHover = EditorGUILayout.ColorField("Normal Hover", curve.s_colorNormalHover);
                curve.s_colorNormalClick = EditorGUILayout.ColorField("Normal Clicked", curve.s_colorNormalClick);
                curve.s_colorNormalDisabled = EditorGUILayout.ColorField("Normal Disabled", curve.s_colorNormalDisabled);
                EditorGUILayout.LabelField("Bitangent Colors");
                curve.s_colorBitangentDefault = EditorGUILayout.ColorField("Bitangent Default", curve.s_colorBitangentDefault);
                curve.s_colorBitangentHover = EditorGUILayout.ColorField("Bitangent Hover", curve.s_colorBitangentHover);
                curve.s_colorBitangentClick = EditorGUILayout.ColorField("Bitangent Clicked", curve.s_colorBitangentClick);
                curve.s_colorBitangentDisabled = EditorGUILayout.ColorField("Bitangent Disabled", curve.s_colorBitangentDisabled);
                EditorGUILayout.LabelField("Anchor Point Colors");
                curve.s_colorPointAnchorDefault = EditorGUILayout.ColorField("Anchor Point Default", curve.s_colorPointAnchorDefault);
                curve.s_colorPointAnchorHover = EditorGUILayout.ColorField("Anchor Point Hover", curve.s_colorPointAnchorHover);
                curve.s_colorPointAnchorClick = EditorGUILayout.ColorField("Anchor Point Clicked", curve.s_colorPointAnchorClick);
                curve.s_colorPointAnchorDisabled = EditorGUILayout.ColorField("Anchor Point Default", curve.s_colorPointAnchorDisabled);
                EditorGUILayout.LabelField("Broken Anchor Point Colors");
                curve.s_colorPointAnchorDiscontinuousDefault = EditorGUILayout.ColorField("Broken Anchor Point Default", curve.s_colorPointAnchorDiscontinuousDefault);
                curve.s_colorPointAnchorDiscontinuousHover = EditorGUILayout.ColorField("Broken Anchor Point Hover", curve.s_colorPointAnchorDiscontinuousHover);
                curve.s_colorPointAnchorDiscontinuousClick = EditorGUILayout.ColorField("Broken Anchor Point Clicked", curve.s_colorPointAnchorDiscontinuousClick);
                curve.s_colorPointAnchorDiscontinuousDisabled = EditorGUILayout.ColorField("Broken Anchor Point Default", curve.s_colorPointAnchorDiscontinuousDisabled);
                EditorGUILayout.LabelField("Handle Point Colors");
                curve.s_colorPointHandleDefault = EditorGUILayout.ColorField("Handle Point Default", curve.s_colorPointHandleDefault);
                curve.s_colorPointHandleHover = EditorGUILayout.ColorField("Handle Point Hover", curve.s_colorPointHandleHover);
                curve.s_colorPointHandleClick = EditorGUILayout.ColorField("Handle Point Clicked", curve.s_colorPointHandleClick);
                curve.s_colorPointHandleDisabled = EditorGUILayout.ColorField("Handle Point Disabled", curve.s_colorPointHandleDisabled);
                EditorGUILayout.LabelField("Control Polygon Colors");
                curve.s_colorControlPolygon = EditorGUILayout.ColorField("Control Polygon", curve.s_colorControlPolygon);
                curve.s_colorControlHull = EditorGUILayout.ColorField("Control Convex Hull", curve.s_colorControlHull);
            }

            EditorGUI.indentLevel--;
        }
        #endregion

        GUILayout.Space(10);
        // Consider giving red color, hatched pattern, etc. to convey danger
        if (GUILayout.Button("Reset Curve"))
        {
            curve.ResetToTemplate(Vector3.zero);

            SceneView.RepaintAll();
            Debug.Log("Clicked Reset Curve!");
        }
    }

    private void SetDefaultDisplayMetrics()
    {
        curve.s_handlePointRadiusFixed = false;
        curve.s_handlePointUniformRadius = 0.25f;
        curve.s_handlePointAnchorRadius = 0.25f;
        curve.s_handlePointAnchorDiscontinuousRadius = 0.15f;
        curve.s_handlePointHandleRadius = 0.25f;

        curve.s_displayWidthFixed = false;
        curve.s_displayWidthUniform = 5f;
        curve.s_displayWidthCurve = 5f;
        curve.s_displayWidthTangent = 5f;
        curve.s_displayWidthNormal = 5f;
        curve.s_displayWidthBitangent = 5f;

        curve.s_displayLengthFixed = false;
        curve.s_displayLengthUniform = 1f;
        curve.s_displayLengthTangent = 1f;
        curve.s_displayLengthNormal = 1f;
        curve.s_displayLengthBitangent = 1f;

        // Repaint();
    }

    private void SetDefaultColors()
    {
        curve.s_colorCurveDefault = new Color(235 / 255f, 235 / 255f, 235 / 255f, 1);
        curve.s_colorCurveHover = new Color(255 / 255f, 255 / 255f, 255 / 255f, 1);
        curve.s_colorCurveClick = new Color(195 / 255f, 195 / 255f, 195 / 255f, 1);
        curve.s_colorCurveDisabled = new Color(155 / 255f, 155 / 255f, 155 / 255f, 1);

        curve.s_colorTangentDefault = new Color(255 / 255f, 55 / 255f, 55 / 255f, 1);
        curve.s_colorTangentHover = new Color(255 / 255f, 75 / 255f, 75 / 255f, 1);
        curve.s_colorTangentClick = new Color(255 / 255f, 15 / 255f, 15 / 255f, 1);
        curve.s_colorTangentDisabled = new Color(175 / 255f, 155 / 255f, 155 / 255f, 1);

        curve.s_colorNormalDefault = new Color(55 / 255f, 255 / 255f, 55 / 255f, 1);
        curve.s_colorNormalHover = new Color(75 / 255f, 255 / 255f, 75 / 255f, 1);
        curve.s_colorNormalClick = new Color(15 / 255f, 255 / 255f, 15 / 255f, 1);
        curve.s_colorNormalDisabled = new Color(155 / 255f, 175 / 255f, 155 / 255f, 1);

        curve.s_colorBitangentDefault = new Color(55 / 255f, 55 / 255f, 255 / 255f, 1);
        curve.s_colorBitangentHover = new Color(75 / 255f, 75 / 255f, 255 / 255f, 1);
        curve.s_colorBitangentClick = new Color(15 / 255f, 15 / 255f, 255 / 255f, 1);
        curve.s_colorBitangentDisabled = new Color(155 / 255f, 155 / 255f, 175 / 255f, 1);

        curve.s_colorPointAnchorDefault = new Color(195 / 255f, 55 / 255f, 55 / 255f, 1);
        curve.s_colorPointAnchorHover = new Color(215 / 255f, 55 / 255f, 55 / 255f, 1);
        curve.s_colorPointAnchorClick = new Color(175 / 255f, 55 / 255f, 55 / 255f, 1);
        curve.s_colorPointAnchorDisabled = new Color(175 / 255f, 155 / 255f, 155 / 255f, 1);

        curve.s_colorPointAnchorDiscontinuousDefault = new Color(195 / 255f, 195 / 255f, 195 / 255f, 1);
        curve.s_colorPointAnchorDiscontinuousHover = new Color(215 / 255f, 215 / 255f, 215 / 255f, 1);
        curve.s_colorPointAnchorDiscontinuousClick = new Color(175 / 255f, 175 / 255f, 175 / 255f, 1);
        curve.s_colorPointAnchorDiscontinuousDisabled = new Color(155 / 255f, 155 / 255f, 155 / 255f, 1);

        curve.s_colorPointHandleDefault = new Color(55 / 255f, 195 / 255f, 55 / 255f, 1);
        curve.s_colorPointHandleHover = new Color(55 / 255f, 215 / 255f, 55 / 255f, 1);
        curve.s_colorPointHandleClick = new Color(55 / 255f, 175 / 255f, 55 / 255f, 1);
        curve.s_colorPointHandleDisabled = new Color(155 / 255f, 175 / 255f, 155 / 255f, 1);

        curve.s_colorControlPolygon = new Color(55 / 255f, 55 / 255f, 55 / 255f, 1);
        curve.s_colorControlHull = new Color(55 / 255f, 55 / 255f, 55 / 255f, 0.1f);

        //Repaint();
    }
}
