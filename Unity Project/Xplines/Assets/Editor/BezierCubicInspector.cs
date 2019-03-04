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
    private AddMode addMode = AddMode.CameraForward;
    /// <summary>
    /// How far along selected AddMode to add new anchor point. 
    /// E.g. x along tangent, x in front of camera, etc. 
    /// </summary>
    private float addModeDistance = 5f;
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
        if (curve == null)
            curve = (BezierCubic)target;
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
            Debug.Log("Clicked Reset Curve!");
        }

        // Editing
        Event e = Event.current;
        // ...
        if (e.shift)
        {
            // If clicking
            if (e.isMouse)
            {
                // Shift-click to add anchor (and auto tangent)

                // Sets OnMouseButtonDown click position
                if (Input.GetMouseButtonDown(0))
                {
                    clickPos = Input.mousePosition;
                }
                // Drag to register delta away from clickPos
                if (Input.GetMouseButton(0))
                {
                    dragDelta = (Vector2)Input.mousePosition - clickPos;
                }
                // If releasing left-click without moving mouse
                if (Input.GetMouseButtonUp(0))
                {
                    // If releasing left-click without moving mouse
                    if (dragDelta.sqrMagnitude == 0f)
                    {
                        switch (addMode)
                        {
                            case AddMode.Collinear:
                            {
                                break;
                            }
                            case AddMode.CameraForward:
                            {
                                Vector3 camPoint
                                    = Camera.main.transform.position
                                    + Camera.main.transform.forward
                                    * addModeDistance;

                                curve.AddSegment(camPoint);
                                break;
                            }
                            case AddMode.XZ_Plane:
                            {
                                Camera cam = Camera.main;
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
                    // Else if releasing left-click after dragging
                    else
                    {

                    }

                    clickPos = Vector2.negativeInfinity;
                    dragDelta = Vector2.negativeInfinity;
                }

                // Shift-click-drag to add anchor and then last tangent (first tangent is auto)
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

        // Displaying
        for (int i = 0; i < curve.CountSegments; i++)
        {
            List<Vector3> p = curve.SegmentPoints(i);
            Handles.DrawBezier(p[0], p[1], p[2], p[3], Color.magenta, null, 2f);
        }
    }
}
