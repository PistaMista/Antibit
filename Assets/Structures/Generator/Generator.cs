using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Structures
{
    /// <summary>
    /// Generates friendly tiles.
    /// </summary>
    public class Generator : Structure
    {
        public Tiles.Tile[] generating_tiles;
        protected override void Form(Tiles.Tile[][] markedTiles)
        {
            base.Form(markedTiles);
            generating_tiles = markedTiles[0];
        }

        public override void Activate()
        {
            foreach (Tiles.Tile tile in generating_tiles)
            {
                if (tile.Owner == null)
                {
                    tile.SetOwner(owner, false);
                }
            }
        }
    }
}