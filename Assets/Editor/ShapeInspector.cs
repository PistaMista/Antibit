using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Shape))]
public class ShapeInspector : Editor
{

    public override void OnInspectorGUI()
    {
        Shape inspected = target as Shape;
        GUILayout.BeginHorizontal();
        Vector2Int dimensions_input = EditorGUILayout.Vector2IntField("Dimensions: ", inspected.baseComposition.size);
        if (inspected.baseComposition.size != dimensions_input)
        {
            inspected.baseComposition = new Shape.Composition(dimensions_input.x, dimensions_input.y);
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("Composition: ");
        for (int y = inspected.baseComposition.size.y - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();

            for (int x = 0; x < inspected.baseComposition.size.x; x++)
            {
                inspected.baseComposition.ownerships[x, y] = (Shape.Ownership)EditorGUILayout.EnumPopup(inspected.baseComposition.ownerships[x, y]);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.Label("Markers: ");
        for (int y = inspected.baseComposition.size.y - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();

            for (int x = 0; x < inspected.baseComposition.size.x; x++)
            {
                inspected.baseComposition.markers[x, y] = (byte)EditorGUILayout.IntField((int)inspected.baseComposition.markers[x, y]);
            }

            GUILayout.EndHorizontal();
        }

        if (GUI.changed)
        {
            inspected.Compose();
            Debug.Log("Recomposing!");
        }
    }
}
