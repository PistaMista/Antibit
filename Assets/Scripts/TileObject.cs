using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Gameplay.Tiles
{
    public class Tile
    {
        private TileObject original;
        public TileObject present;
        readonly Transform parent;

        public readonly Vector2Int position;
        private Tile[] _neighbours;

        public ReadOnlyCollection<Tile> neighbours
        {
            get
            {
                return Array.AsReadOnly(_neighbours);
            }
        }

        public Tile(Vector2Int position, GameObject prefab, Transform parent, ref Action post_initializer)
        {
            this.position = position;

            original = GameObject.Instantiate(prefab).GetComponent<TileObject>();
            present = GameObject.Instantiate(prefab).GetComponent<TileObject>();
            this.parent = parent;

            present.transform.SetParent(parent);
            original.transform.SetParent(present.transform);

            post_initializer += AssignNeighbours;
        }

        private void AssignNeighbours()
        {
            _neighbours = new Tile[4];
            for (int i = 0; i < 4; i++)
            {
                Vector2Int pos = position + new Vector2Int((i - 1) % 2, (i - 2) % 2);
                if (pos.x >= 0 && pos.x < Board.board.tiles.GetLength(0) && pos.y >= 0 && pos.y < Board.board.tiles.GetLength(1))
                {
                    _neighbours[i] = Board.board[pos.x, pos.y];
                }
            }
        }

        public void ApplyChange()
        {
            if (original != present)
            {
                //Shape.Ghosts.OnTileChange(this);

                GameObject.Destroy(original.gameObject);

                original = present;
                present = GameObject.Instantiate(present.gameObject, present.transform.parent).GetComponent<TileObject>();

                original.transform.SetParent(present.transform);
            }
        }
    }
    public class TileObject : MonoBehaviour
    {
        private Player owner;
        public Player Owner
        {
            get
            {
                return owner;
            }
        }
    }
}
