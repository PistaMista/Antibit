using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor
{
    [CustomEditor(typeof(Gameplay.Board))]
    public class BoardInspector : Editor
    {
        Gameplay.Board board;
        Vector2Int new_dimensions;
        void Awake()
        {
            board = target as Gameplay.Board;
            new_dimensions = board.dimensions;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            new_dimensions = EditorGUILayout.Vector2IntField("New dimensions: ", new_dimensions);

            if (GUILayout.Button("Regenerate") && EditorUtility.DisplayDialog("Board Regeneration", "Are you sure you want to regenerate the board?", "Regenerate", "Cancel")) RegenerateBoard();
        }

        void RegenerateBoard()
        {
            if (board.tiles != null)
            {
                for (int x = 0; x < board.dimensions.x; x++)
                    for (int y = 0; y < board.dimensions.y; y++)
                        if (board.tiles[x, y] != null)
                            Destroy(board.tiles[x, y].gameObject);
            }

            board.tiles = new Gameplay.Tile[new_dimensions.x, new_dimensions.y];
        }
    }
}