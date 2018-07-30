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
            Shape.InitializeAll();
        }

        public Tile tilePrefab;
        public Structure[] structurePrefabs;
        public float paddingToTileRatio;
        public Vector2Int startingTileRectangleSize;
        public int playerOriginYIndent;
        Tile[,] tiles;
        Dictionary<int, Structure> structures;
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
        public Vector2Int center;
        public Vector2Int size;
        public float tileSize;
        public static void Renew(int board_side_length)
        {
            if (board_side_length % 2 == 0 || board_side_length <= 1) throw new System.Exception("Board size invalid.");
            if (board.tiles != null)
                for (int x = 0; x < board.tiles.GetLength(0); x++)
                    for (int y = 0; y < board.tiles.GetLength(1); y++)
                        Destroy(board[x, y].gameObject);

            if (board.structures != null)
                foreach (KeyValuePair<int, Structure> item in board.structures)
                    Destroy(item.Value.gameObject);


            float board_size = board.GetComponent<RectTransform>().sizeDelta.x;
            board.tileSize = board_size / board_side_length;
            GridLayoutGroup grid = board.GetComponent<GridLayoutGroup>();

            grid.cellSize = Vector2.one * board.tileSize * (1.0f - board.paddingToTileRatio);
            grid.spacing = Vector2.one * board.tileSize * board.paddingToTileRatio;

            board.center = Vector2Int.one * (board_side_length / 2);
            board.size = Vector2Int.one * board_side_length;

            board.tiles = new Tile[board_side_length, board_side_length];
            for (int x = 0; x < board_side_length; x++)
            {
                for (int y = 0; y < board_side_length; y++)
                {
                    Tile tile = Instantiate(board.tilePrefab, board.transform).GetComponent<Tile>();
                    tile.transform.localScale = Vector3.one;
                    tile.position = new Vector2Int(x, y);

                    tile.GetComponent<Button>().onClick.AddListener(() => OnTileClick(tile.position));
                    board[x, y] = tile;
                }
            }

            foreach (Tile tile in board.tiles)
            {
                tile.neighbours = new Tile[4];
                for (int i = 0; i < 4; i++)
                {
                    Vector2Int pos = tile.position + new Vector2Int((i - 1) % 2, (i - 2) % 2);
                    if (pos.x >= 0 && pos.x < board.tiles.GetLength(0) && pos.y >= 0 && pos.y < board.tiles.GetLength(1))
                    {
                        tile.neighbours[i] = board[pos.x, pos.y];
                    }
                }
            }

            board.structures = new Dictionary<int, Structure>();
            Shape.InitializeGhosts();

            Player.ReinitializePlayers(true);

            Vector2Int player_origin = new Vector2Int(Mathf.FloorToInt(board_side_length / 2.0f), 0);
            Vector2Int player_rectangle_start = new Vector2Int(player_origin.x - Mathf.FloorToInt(board.startingTileRectangleSize.x / 2.0f), 0);
            Vector2Int player_rectangle_end = new Vector2Int(player_rectangle_start.x + board.startingTileRectangleSize.x - 1, 0);
            for (int i = 0; i < 2; i++)
            {
                Player player = Player.players[i];
                int rect_y_half = Mathf.FloorToInt(board.startingTileRectangleSize.y / 2.0f);
                player_rectangle_start.y = i == 0 ? (board.playerOriginYIndent - rect_y_half) : (board.size.y - 1 - board.playerOriginYIndent - rect_y_half);
                player_rectangle_end.y = player_rectangle_start.y + board.startingTileRectangleSize.y - 1;
                player_origin.y = player_rectangle_start.y + rect_y_half;

                player.origin = board[player_origin.x, player_origin.y];

                for (int x = player_rectangle_start.x; x <= player_rectangle_end.x; x++)
                {
                    for (int y = player_rectangle_start.y; y <= player_rectangle_end.y; y++)
                    {
                        board[x, y].SetOwner(player, false);
                    }
                }
            }
        }

        void Update()
        {
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(i.ToString()))
                {
                    Debug.Log("Pressed " + i * 9);
                    Board.Renew(i * 9);
                }
            }
        }

        static void OnTileClick(Vector2Int pos)
        {
            Debug.Log("Clicked tile " + pos.x + " " + pos.y);
            if (!Player.player_on_turn.ai)
            {
                Tile tile = board[pos.x, pos.y];
                if (!UI.View.zoomed)
                {
                    UI.View.ZoomInto(pos);
                }
                else
                {
                    if (Player.player_on_turn.sources.Is(tile) && tile != Player.source)
                        tile.SelectSource();
                    else
                    {
                        UI.View.Unzoom();
                        if (Player.source != null && Player.player_on_turn.destinations.Is(tile))
                            tile.SelectDestination();
                    }
                }

            }
        }
    }
}