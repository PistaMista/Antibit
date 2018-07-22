using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay;

namespace UI
{
    public class View : MonoBehaviour
    {
        public static bool zoomed;
        static float board_scale;
        static View view;
        public int zoomSize;
        RectTransform rect;
        RectTransform boardRect;
        void Start()
        {
            view = this;
            rect = GetComponent<RectTransform>();
            boardRect = Board.board.GetComponent<RectTransform>();

            board_scale = boardRect.rect.height / rect.rect.height;
        }

        public static void ZoomInto(Vector2Int position)
        {
            if (zoomed) return;
            float scaledTileSize = view.rect.localScale.x * Board.board.tileSize;
            Camera.main.transform.position = new Vector3(position.x - Board.board.center.x, position.y - Board.board.center.y, 0) * scaledTileSize;
            Camera.main.orthographicSize = (float)view.zoomSize / Board.board.size.y * board_scale;

            zoomed = true;
        }

        public static void Unzoom()
        {
            if (!zoomed) return;
            Camera.main.transform.position = Vector3.zero;
            Camera.main.orthographicSize = 1;
            zoomed = false;
        }
    }
}