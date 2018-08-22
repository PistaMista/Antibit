using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Structures
{
    public class Siphon : Generator
    {
        protected override void Form(Tile[][] markedTiles)
        {
            base.Form(markedTiles);
            Activate();
        }

        public override void Activate()
        {
            base.Activate();
            for (int i = 0; i < generating_tiles.Length; i++)
            {
                Tile tile = generating_tiles[i];
                if (!tile.Owner.free_tiles.Contains(tile)) tile.Owner.free_tiles.Add(tile);
            }
        }

    }
}