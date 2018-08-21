using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Structures
{
    public class Siphon : Generator, Fragile
    {
        public void Shatter()
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    Tile tile = tiles[x, y];
                    if (tile != null && tile.Owner == owner) tile.SetOwner(null, checkIntegrity: true);
                }
            }

            for (int i = 0; i < generating_tiles.Length; i++)
            {
                Tile tile = generating_tiles[i];
                if (tile != null) tile.SetOwner(null, checkIntegrity: true);
            }
        }

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