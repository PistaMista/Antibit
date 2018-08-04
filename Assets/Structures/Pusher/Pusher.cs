using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameplay.Structures
{
    public class Pusher : Structure
    {
        public Tile[] pushedTiles;
        public Vector2Int pushDirection;
        protected override void Form(Tile[][] markedTiles)
        {
            base.Form(markedTiles);
            Tile directionalIndicator = markedTiles[1].Single();
            pushedTiles = markedTiles[0].OrderBy(x => Vector2.Distance(directionalIndicator.position, x.position)).ToArray();
            pushDirection = directionalIndicator.position - pushedTiles[0].position;
        }

        public override void Activate()
        {
            foreach (Tile tile in pushedTiles)
            {
                if (tile.Owner != null)
                {
                    Tile.Push(tile, pushDirection);
                }
            }
        }
    }
}