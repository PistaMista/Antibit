using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Shape))]
public class ShapeInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Shape shape = target as Shape;
        shape.footprint = EditorGUILayout.Vector2IntField("Dimensions: ", shape.footprint);
    }
}
