using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Bézier functionality: 
    - Explicit formula
    - Explicit derivative formula
    - Explicit double derivative formula
    - deCasteljau algorithm
    - Curvature
    - Arc length
    - Closest point on curve to world point
    - Bounding box
    - Intersection testing: 
    - - Curve-curve intersection
    - - Curve-line intersection
    - - Curve-circle intersection
    - Functions: 
    - - Curve fitting (points)
    - - Curve fitting (points with radius)
    - - Curve fitting (functions)
    - - Function from curve
    - Project onto shape (e.g. terrain)
    - Inflection points
    - Extrema
    - Events: 
    - - Event point
    - - Event interval
    - Physics: 
    - - Gravity on points
    - - - Tangents correctly adjusting
    - - - Bounding and stretching
    - - Joints: 
    - - - Hinge joint
    - - - Bend joint (think about a ruler being bent)

    Editing: 
    - Add control point (not tangent point)
    - Add and drag to add control point + tangent
    - Click to add control point and auto tangent
    - Drag control point
    - Add to terrain below, or else onto the xz-plane
    - Remove point
    - Split segment
    - Close curve
    - Bézier tool active overlay (to show you're editing)
    - Import SVG
    - Export SVG
    - Break tangents
    - Lock tangents
    - Restore tangents
    - 3D world editing
    - - Also applies for 2D mode (check if 2D is toggled in scene view)
    - 2D editor window editing 
    - 2D inspector editing

    Settings:
    - Handles
    - - Control point size
    - - Control point type stylings
    - - 

    Analysis functionality: 
    - Curvature overlay
    - Show tangents
    - Show normals
    - Show bitangents
    - Show length from selected
    - Computational complexity
*/

[System.Serializable]
public class BezierCurve : MonoBehaviour
{
    #region Variables
    #endregion

    #region Functionality
    #endregion
}
