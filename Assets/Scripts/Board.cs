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

            tiles = new Tiles.TileObject[size.x, size.y];
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector3Int position = origin + new Vector3Int(x, y, 0);
                    UnityEngine.Tilemaps.Tile render_tile = renderer.GetTile<UnityEngine.Tilemaps.Tile>(position);

                    if (render_tile != null)
                    {
                        Tiles.TileObject tile = Instantiate(tilePrefabs.First(candidate => candidate.name == render_tile.name).gameObject).GetComponent<Tiles.TileObject>();
                        tile.transform.SetParent(parent.transform);

                        tile.position = new Vector2Int(x, y);

                        tiles[x, y] = tile;
                    }
                }
            }

        }
        public Tiles.TileObject[] tilePrefabs;
        public Structure[] structurePrefabs;
        public Tiles.Tile[,] tiles;
        public Dictionary<uint, Structure> structures;
        public Tiles.Tile this[int a, int b]
        {
            get
            {
                return tiles[a, b];
            }
        }
    }
}