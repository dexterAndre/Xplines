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

    private float s_curveWidth = 5f;
    private Color s_curveColor = Color.magenta;
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
        if (curve.CountSegments <= 0)
        {
            curve.ResetToTemplate(Vector3.zero);
        }
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

                        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
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
        for (int i = 0; i < curve.CountSegments; i++)
        {
            List<Vector3> p = curve.SegmentPoints(i);
            Handles.DrawBezier(p[0], p[3], p[1], p[2], s_curveColor, null, s_curveWidth);
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

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        if (curve == null)
            curve = (BezierCubic)target;
        SetToggleButtonStyles();

        // Instructions foldout
        if (GUILayout.Button("Edit Curve", isEditing == true ? ToggleButtonStyleOn : ToggleButtonStyleOff))
        {
            isEditing = !isEditing;
            Debug.Log("Clicked Edit Curve!");
        }
        addMode = (AddMode)EditorGUILayout.EnumPopup("Add Mode: ", addMode);
        GUILayout.Space(10);
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
        GUILayout.Space(5);
        if (GUILayout.Button("Reset Curve"))
        {
            curve.ResetToTemplate(Vector3.zero);
            SceneView.RepaintAll();
            Debug.Log("Clicked Reset Curve!");
        }
    }
}
