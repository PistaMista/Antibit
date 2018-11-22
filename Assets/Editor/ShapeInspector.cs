using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UnityEditor
{
    [CustomEditor(typeof(Shape))]
    public class ShapeInspector : Editor
    {
        Vector2Int footprint;
        Shape.Ownership[] ownerships;
        byte[] markers;
        void Awake()
        {
            footprint = serializedObject.FindProperty("footprint").vector2IntValue;

            SerializedProperty ownershipProp = serializedObject.FindProperty("ownershipSequence");
            SerializedProperty markerProp = serializedObject.FindProperty("markerSequence");

            ownerships = new Shape.Ownership[ownershipProp.arraySize];
            markers = new byte[markerProp.arraySize];

            for (int i = 0; i < ownershipProp.arraySize; i++)
            {
                ownerships[i] = (Shape.Ownership)ownershipProp.GetArrayElementAtIndex(i).enumValueIndex;
                markers[i] = (byte)markerProp.GetArrayElementAtIndex(i).intValue;
            }
        }

        public override void OnInspectorGUI()
        {
            Vector2Int dimensions_input = EditorGUILayout.Vector2IntField("Dimensions: ", footprint);
            if (dimensions_input != footprint)
            {
                footprint = dimensions_input;
                serializedObject.FindProperty("footprint").vector2IntValue = footprint;
                ownerships = new Shape.Ownership[footprint.x * footprint.y];
                markers = new byte[footprint.x * footprint.y];
            }

            GUILayout.Label("Composition: ");
            for (int y = footprint.y - 1; y >= 0; y--)
            {
                GUILayout.BeginHorizontal();

                for (int x = 0; x < footprint.x; x++)
                {
                    int index = x + y * footprint.x;
                    ownerships[index] = (Shape.Ownership)EditorGUILayout.EnumPopup(ownerships[index]);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Markers: ");
            for (int y = footprint.y - 1; y >= 0; y--)
            {
                GUILayout.BeginHorizontal();

                for (int x = 0; x < footprint.x; x++)
                {
                    int index = x + y * footprint.x;
                    markers[index] = (byte)EditorGUILayout.IntField(markers[index]);
                }

                GUILayout.EndHorizontal();
            }

            if (GUI.changed)
            {
                SerializedProperty ownershipProp = serializedObject.FindProperty("ownershipSequence");
                SerializedProperty markerProp = serializedObject.FindProperty("markerSequence");

                ownershipProp.ClearArray();
                markerProp.ClearArray();

                for (int i = 0; i < ownerships.Length; i++)
                {
                    ownershipProp.InsertArrayElementAtIndex(i);
                    ownershipProp.GetArrayElementAtIndex(i).enumValueIndex = (int)ownerships[i];

                    markerProp.InsertArrayElementAtIndex(i);
                    markerProp.GetArrayElementAtIndex(i).intValue = (byte)markers[i];
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}