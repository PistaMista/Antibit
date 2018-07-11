using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class Board : MonoBehaviour
    {
        public static Board board;
        void Start()
        {
            board = this;
        }
        public Tile tilePrefab;
        public float paddingToTileRatio;
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
        public Vector2Int size
        {
            get
            {
                return new Vector2Int(tiles.GetLength(0), tiles.GetLength(1));
            }
        }
        public static void Renew(int board_side_length)
        {
            Player.ReinitializePlayers(true);

            if (board_side_length % 2 == 0 || board_side_length <= 1) throw new System.Exception("Board size invalid.");
            if (board.tiles != null)
                for (int x = 0; x < board.tiles.GetLength(0); x++)
                    for (int y = 0; y < board.tiles.GetLength(1); y++)
                        Destroy(board[x, y].gameObject);

            float board_size = board.GetComponent<RectTransform>().sizeDelta.x;
            float tile_size = board_size / (board_side_length + 2);
            GridLayoutGroup grid = board.GetComponent<GridLayoutGroup>();

            grid.cellSize = Vector2.one * tile_size * (1.0f - board.paddingToTileRatio);
            grid.spacing = Vector2.one * tile_size * board.paddingToTileRatio;

            board.tiles = new Tile[board_side_length + 2, board_side_length + 2];
            int player_x_spawn = Mathf.CeilToInt(board_side_length / 2.0f);
            for (int x = 0; x < board_side_length + 2; x++)
            {
                for (int y = 0; y < board_side_length + 2; y++)
                {
                    Tile tile = Instantiate(board.tilePrefab, board.transform).GetComponent<Tile>();
                    tile.transform.localScale = Vector3.one;
                    tile.position = new Vector2Int(x, y);

                    if (x == player_x_spawn && (y == 0 || y == board_side_length + 1)) tile.Owner = Player.players[y == 0 ? 0 : 1];
                    else
                    if (x == 0 || x == board_side_length + 1 || y == 0 || y == board_side_length + 1) tile.Cost = int.MaxValue;
                    else tile.UpdateColor();

                    tile.GetComponent<Button>().onClick.AddListener(() => OnTileClick(tile.position));
                    board[x, y] = tile;
                }
            }

            Player.Next();
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

        static void OnTileClick(Vector2Int pos)
        {
            Debug.Log("Clicked tile " + pos.x + " " + pos.y);
            if (!Player.player_on_turn.ai)
            {
                Tile tile = board[pos.x, pos.y];
                if (tile.cost != 0 && tile.cost <= Player.player_on_turn.bits) tile.Take();
                Debug.Log("Bits remaining " + Player.player_on_turn.bits);
            }
        }
    }
}