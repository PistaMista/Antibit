﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Gameplay
{
    public class Structure : MonoBehaviour
    {
        public Shape[] shapes;
        public Player owner;
        Tile[,] tiles;


        public Tile position;

        public void Form(Tile tile, bool[,] formation, int[,] markers, int marker_group_count)
        {
            tiles = new Tile[formation.GetLength(0), formation.GetLength(1)];
            List<Tile>[] markedTiles = new List<Tile>[marker_group_count];
            for (int i = 0; i < marker_group_count; i++)
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
        }

        public virtual void Activate()
        {

        }
    }
}