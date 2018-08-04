using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Structures
{
    public class Generator : Structure
    {
        public Tile[] generating_tiles;
        protected override void Form(Tile[][] markedTiles)
        {
            base.Form(markedTiles);
            generating_tiles = markedTiles[0];
        }

        public override void Activate()
        {
            foreach (Tile tile in generating_tiles)
            {
                if (tile.Owner == null)
                {
                    tile.SetOwner(owner, false);
                }
            }
        }
    }
}