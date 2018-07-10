using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class Board : MonoBehaviour
    {
        static Board board;
        void Start()
        {
            board = this;
        }
        public Tile tile_prefab;
        public float padding_to_tile_ratio;
        Tile[,] tiles;
        public Tile this[int a, int b]
        {
            get
            {
                return tiles[a, b];
            }
            set
            {
                tiles[a, b] = value;
            }
        }
        public static void Renew(int size)
        {
            if (size % 2 == 0 || size <= 1) throw new System.Exception("Board size invalid.");
            for (int x = 0; x < board.tiles.GetLength(0); x++)
                for (int y = 0; y < board.tiles.GetLength(1); y++)
                    Destroy(board[x, y].gameObject);

            float rect_size = board.GetComponent<RectTransform>().sizeDelta.x;
            float tile_size = rect_size / size;
            GridLayoutGroup grid = board.GetComponent<GridLayoutGroup>();

            grid.cellSize = Vector2.one * tile_size * (1.0f - board.padding_to_tile_ratio);
            grid.spacing = Vector2.one * tile_size * board.padding_to_tile_ratio;

            board.tiles = new Tile[size + 1, size + 1];
            int player_x_spawn = Mathf.CeilToInt(size / 2.0f);
            for (int x = 0; x <= size; x++)
            {
                for (int y = 0; y <= size; y++)
                {
                    Tile tile = Instantiate(board.tile_prefab).GetComponent<Tile>();
                    tile.transform.SetParent(board.transform);



                    board[x, y] = tile;
                }
            }
        }

        void Update()
        {
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(i.ToString()))
                {
                    Debug.Log("Pressed " + i);
                    Board.Renew(i);
                }
            }
        }
    }
}