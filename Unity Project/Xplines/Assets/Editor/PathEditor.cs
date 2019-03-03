using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    private PathCreator _creator;
    private Path _path;

    private void OnEnable()
    {
        _creator = (PathCreator)target;
        if (_creator._path == null)
        {
            _creator.CreatePath();
        }
        _path = _creator._path;
    }

    private void OnSceneGUI()
    {
        Input();
        Draw();
    }

    private void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Undo.RecordObject(_creator, "Add Segment");
            _path.AddSegment(mousePos);
        }
    }

    private void Draw()
    {
        // Drawing curve segments
        for (int i = 0; i < _path.CountSegments; i++)
        {
            Vector2[] points = _path.GetSegmentPoints(i);

            Handles.color = _creator._colorHandleLine;
            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[2], points[3]);

            Handles.DrawBezier(points[0], points[3], points[1], points[2], _creator._colorCurve, null, 2);
        }

        // Drawing control points
        Handles.color = _creator._colorHandlePoint;
        for (int i = 0; i < _path.CountPoints; i++)
        {
            Vector2 newPos = Handles.FreeMoveHandle(
                _path[i], 
                Quaternion.identity, 
                0.1f, 
                Vector2.zero, 
                Handles.CylinderHandleCap);

            if (_path[i] != newPos)
            {
                Undo.RecordObject(_creator, "Move Point");
                _path.RepositionPoint(i, newPos);
            }
        }
    }
}
