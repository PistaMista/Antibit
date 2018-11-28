using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Gameplay.Structures;
using Gameplay.Tiles;

namespace Gameplay
{
    public class Structure : MonoBehaviour
    {
        public Shape[] shapes;
        public Player owner;
        protected TileObject[,] tiles;


        public TileObject position;

        public void Form(TileObject tile, bool[,] formation, int[,] markers, int highest_marker)
        {
            tiles = new TileObject[formation.GetLength(0), formation.GetLength(1)];
            List<TileObject>[] markedTiles = new List<TileObject>[highest_marker];
            for (int i = 0; i < highest_marker; i++)
            {
                markedTiles[i] = new List<TileObject>();
            }

            for (int x = 0; x < formation.GetLength(0); x++)
            {
                for (int y = 0; y < formation.GetLength(1); y++)
                {
                    TileObject t = Board.board[tile.position.x + x, tile.position.y + y];

                    if (formation[x, y])
                    {
                        t.structure = this;
                        tiles[x, y] = t;

                        t.RefreshColor();
                    }

                    int marker = markers[x, y];

                    if (marker > 0)
                    {
                        markedTiles[marker - 1].Add(t);
                    }
                }
            }

            position = tile;

            Form(Array.ConvertAll(markedTiles, x => x.ToArray()));
        }

        protected virtual void Form(TileObject[][] markedTiles)
        {

        }

        public virtual void Deform()
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    TileObject tile = tiles[x, y];
                    if (tile != null && tile.structure == this) tile.structure = null;
                }
            }

            if (this is SA_UNSTABLE) (this as SA_UNSTABLE).DestroyTilesAfterDeformation();
        }

        public virtual void Activate()
        {

        }
    }
}