using System.Collections;
using System.Collections.Generic;

using Gameplay.Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

namespace Gameplay
{
    public class Board : MonoBehaviour
    {
        public static Board board;
        void Start()
        {
            board = this;
            Shape.ComposeAll();



            GameObject parent = new GameObject("Tiles");
            parent.transform.SetParent(this.transform);

            Tilemap renderer = GetComponentInChildren<Tilemap>();
            Vector3Int size = renderer.size;
            Vector3Int origin = renderer.origin;

            tiles = new Tiles.Tile[size.x, size.y];
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector3Int position = origin + new Vector3Int(x, y, 0);
                    UnityEngine.Tilemaps.Tile render_tile = renderer.GetTile<UnityEngine.Tilemaps.Tile>(position);

                    if (render_tile != null)
                    {
                        Tiles.Tile tile = Instantiate(tilePrefabs.First(candidate => candidate.name == render_tile.name).gameObject).GetComponent<Tiles.Tile>();
                        tile.transform.SetParent(parent.transform);

                        tile.position = new Vector2Int(x, y);

                        tiles[x, y] = tile;
                    }
                }
            }

        }
        public Tiles.Tile[] tilePrefabs;
        public Structure[] structurePrefabs;
        public Tiles.Tile[,] tiles;
        public Dictionary<uint, Structure> structures;
        public Tiles.Tile this[int a, int b]
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
        // public void Renew(int board_side_length)
        // {
        //     if (board_side_length % 2 == 0 || board_side_length <= 1) throw new System.Exception("Board size invalid.");
        //     if (board.tiles != null)
        //         for (int x = 0; x < board.tiles.GetLength(0); x++)
        //             for (int y = 0; y < board.tiles.GetLength(1); y++)
        //                 Destroy(board[x, y].gameObject);

        //     if (board.structures != null)
        //         foreach (KeyValuePair<uint, Structure> item in board.structures)
        //             Destroy(item.Value.gameObject);


        //     float board_size = board.GetComponent<RectTransform>().sizeDelta.x;
        //     board.tileSize = board_size / board_side_length;
        //     GridLayoutGroup grid = board.GetComponent<GridLayoutGroup>();

        //     grid.cellSize = Vector2.one * board.tileSize * (1.0f - board.paddingToTileRatio);
        //     grid.spacing = Vector2.one * board.tileSize * board.paddingToTileRatio;

        //     board.center = Vector2Int.one * (board_side_length / 2);
        //     board.size = Vector2Int.one * board_side_length;

        //     board.tiles = new Tile[board_side_length, board_side_length];
        //     for (int x = 0; x < board_side_length; x++)
        //     {
        //         for (int y = 0; y < board_side_length; y++)
        //         {
        //             Tile tile = Instantiate(board.tilePrefab, board.transform).GetComponent<Tile>();
        //             tile.transform.localScale = Vector3.one;
        //             tile.position = new Vector2Int(x, y);

        //             tile.GetComponent<Button>().onClick.AddListener(() => OnTileClick(tile.position));
        //             board[x, y] = tile;
        //         }
        //     }

        //     foreach (Tile tile in board.tiles)
        //     {
        //         tile.neighbours = new Tile[4];
        //         for (int i = 0; i < 4; i++)
        //         {
        //             Vector2Int pos = tile.position + new Vector2Int((i - 1) % 2, (i - 2) % 2);
        //             if (pos.x >= 0 && pos.x < board.tiles.GetLength(0) && pos.y >= 0 && pos.y < board.tiles.GetLength(1))
        //             {
        //                 tile.neighbours[i] = board[pos.x, pos.y];
        //             }
        //         }
        //     }

        //     board.structures = new Dictionary<uint, Structure>();

        //     Player.ReinitializePlayers(true);

        //     Shape.Ghosts.Add();

        //     Vector2Int player_origin = new Vector2Int(Mathf.FloorToInt(board_side_length / 2.0f), 0);
        //     for (int i = 0; i < 2; i++)
        //     {
        //         Player player = Player.players[i];
        //         player_origin.y = i == 0 ? Board.board.playerMainBaseCenterIndent : Board.board.size.y - 1 - Board.board.playerMainBaseCenterIndent;

        //         Board.board.structurePrefabs.First(x => x is Structures.Base).shapes[0].Materialise(player_origin, owner: player, centered: true);
        //         Structures.Base player_base = Board.board.structures.Values.First(x => x is Structures.Base && x.owner == player) as Structures.Base;
        //         player_base.main = true;
        //     }
        // }

        static void OnTileClick(Vector2Int pos)
        {
            Debug.Log("Clicked tile " + pos.x + " " + pos.y);
            if (!Player.player_on_turn.ai)
            {
                Tiles.Tile tile = board[pos.x, pos.y];
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
                        if (Player.player_on_turn.destinations.Is(tile))
                        {
                            if (Player.source != null)
                            {
                                tile.SelectDestination();
                                UI.View.Unzoom();
                            }
                            else if (Player.player_on_turn.free_tiles.Count > 0)
                            {
                                Player.player_on_turn.free_tiles.First().SelectSource();
                                tile.SelectDestination();
                            }
                        }
                        else
                        {
                            UI.View.Unzoom();
                        }
                    }
                }
            }
        }
    }
}