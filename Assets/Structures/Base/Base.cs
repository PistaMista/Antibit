using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameplay.Structures
{
    /// <summary>
    /// The base structure of each player. Players lose after their base is deformed.
    /// </summary>
    public class Base : Generator, SA_UNSTABLE, SA_CONDITIONAL
    {
        public bool IsFormableFor(Player player)
        {
            return !player.structures.Any(x => x is Base);
        }
        public bool IsDeformable()
        {
            return true;
        }
        public bool main = false;
        public void DestroyTilesAfterDeformation()
        {
            main = false;

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    Tiles.Tile tile = tiles[x, y];
                    if (tile != null) tile.SetOwner(null, checkIntegrity: true);
                }
            }
        }

        protected override void Form(Tiles.Tile[][] markedTiles)
        {
            base.Form(markedTiles);
            Activate();
        }

        public override void Activate()
        {
            base.Activate();
            for (int i = 0; i < generating_tiles.Length; i++)
            {
                Tiles.Tile tile = generating_tiles[i];
                if (!tile.Owner.free_tiles.Contains(tile)) tile.Owner.free_tiles.Add(tile);
            }
        }
    }
}
