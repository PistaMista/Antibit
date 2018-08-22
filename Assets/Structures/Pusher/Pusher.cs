using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameplay.Structures
{
    /// <summary>
    /// Pushes tiles forward, creating a stream of tiles used for damaging enemy infrastructure.
    /// </summary>
    public class Pusher : Structure
    {
        public Tile[] pushedTiles;
        public Vector2Int pushDirection;
        protected override void Form(Tile[][] markedTiles)
        {
            base.Form(markedTiles);
            Tile directionalIndicator = markedTiles[1].Single();
            pushedTiles = markedTiles[0].Union(markedTiles[1]).OrderBy(x => Vector2.Distance(directionalIndicator.position, x.position)).ToArray();
            pushDirection = directionalIndicator.position - pushedTiles[1].position;
        }

        public override void Activate()
        {
            Tile pushedTile = pushedTiles.LastOrDefault(x => x.Owner != null);
            if (pushedTile != null) Tile.Push(pushedTile, pushDirection);
        }
    }
}