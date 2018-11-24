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
        public Tiles.Tile[] pushedTiles;
        public Vector2Int pushDirection;
        protected override void Form(Tiles.Tile[][] markedTiles)
        {
            base.Form(markedTiles);
            Tiles.Tile directionalIndicator = markedTiles[1].Single();
            pushedTiles = markedTiles[0].Union(markedTiles[1]).OrderBy(x => Vector2.Distance(directionalIndicator.position, x.position)).ToArray();
            pushDirection = directionalIndicator.position - pushedTiles[1].position;
        }

        public override void Activate()
        {
            Tiles.Tile pushedTile = pushedTiles.LastOrDefault(x => x.Owner != null);
            if (pushedTile != null) Tiles.Tile.Push(pushedTile, pushDirection);
        }
    }
}