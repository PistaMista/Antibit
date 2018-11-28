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
        public Tiles.TileObject[] generating_tiles;
        protected override void Form(Tiles.TileObject[][] markedTiles)
        {
            base.Form(markedTiles);
            generating_tiles = markedTiles[0];
        }

        public override void Activate()
        {
            foreach (Tiles.TileObject tile in generating_tiles)
            {
                if (tile.Owner == null)
                {
                    tile.SetOwner(owner, false);
                }
            }
        }
    }
}