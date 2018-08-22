using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Gameplay.Structures;

namespace Gameplay
{
    public class Structure : MonoBehaviour
    {
        public Shape[] shapes;
        public Player owner;
        protected Tile[,] tiles;


        public Tile position;

        public void Form(Tile tile, bool[,] formation, int[,] markers, int highest_marker)
        {
            tiles = new Tile[formation.GetLength(0), formation.GetLength(1)];
            List<Tile>[] markedTiles = new List<Tile>[highest_marker];
            for (int i = 0; i < highest_marker; i++)
            {
                markedTiles[i] = new List<Tile>();
            }

            for (int x = 0; x < formation.GetLength(0); x++)
            {
                for (int y = 0; y < formation.GetLength(1); y++)
                {
                    Tile t = Board.board[tile.position.x + x, tile.position.y + y];

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

        protected virtual void Form(Tile[][] markedTiles)
        {

        }

        public virtual void Deform()
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    Tile tile = tiles[x, y];
                    if (tile != null && tile.structure == this) tile.structure = null;
                }
            }

            if (this is UNSTABLE) (this as UNSTABLE).DestroyTilesAfterDeformation();
        }

        public virtual void Activate()
        {

        }
    }
}