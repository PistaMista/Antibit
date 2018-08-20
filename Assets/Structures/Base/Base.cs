using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Structures
{
    public class Base : Generator
    {
        public bool main = false;
        public override void Deform()
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    Tile tile = tiles[x, y];
                    if (tile != null) tile.SetOwner(null, checkIntegrity: true);
                }
            }

            base.Deform();
        }

        protected override void Form(Tile[][] markedTiles)
        {
            base.Form(markedTiles);
            Activate();
        }
    }
}
