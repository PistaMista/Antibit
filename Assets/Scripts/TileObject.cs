﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Gameplay.Tiles
{
    public class Tile
    {
        private TileObject original;
        public TileObject present;
        readonly Transform parent;

        public readonly Vector2Int position;
        private Tile[] _neighbours;

        public ReadOnlyCollection<Tile> neighbours
        {
            get
            {
                return Array.AsReadOnly(_neighbours);
            }
        }

        public Tile(Vector2Int position, GameObject prefab, Transform parent, ref Action post_initializer)
        {
            this.position = position;

            original = GameObject.Instantiate(prefab).GetComponent<TileObject>();
            present = GameObject.Instantiate(prefab).GetComponent<TileObject>();
            this.parent = parent;

            original.gameObject.transform.SetParent(parent);
            present.gameObject.transform.SetParent(parent);

            post_initializer += AssignNeighbours;
        }

        private void AssignNeighbours()
        {
            _neighbours = new Tile[4];
            for (int i = 0; i < 4; i++)
            {
                Vector2Int pos = position + new Vector2Int((i - 1) % 2, (i - 2) % 2);
                if (pos.x >= 0 && pos.x < Board.board.tiles.GetLength(0) && pos.y >= 0 && pos.y < Board.board.tiles.GetLength(1))
                {
                    _neighbours[i] = Board.board[pos.x, pos.y];
                }
            }
        }

        public void ApplyChange()
        {
            if (original != present)
            {
                Shape.Ghosts.OnTileChange(this);
            }
        }
    }
    public class TileObject : MonoBehaviour
    {
        private Player owner;
        public Player Owner
        {
            get
            {
                return owner;
            }
        }

        public static Action<TileObject, Player, Player> OnTileChange;

        public void SetOwner(Player player, bool checkIntegrity)
        {
            if (owner != player)
            {
                Player last_owner = owner;
                owner = player;

                if (last_owner != null && last_owner.free_tiles.Contains(this)) last_owner.free_tiles.Remove(this);

                RecalculateSourceAndDestinationProvision();
                if (OnTileChange != null) OnTileChange(this, last_owner, owner);


                if (checkIntegrity) CheckNeighbourIntegrity();
            }
        }

        private void CheckNeighbourIntegrity()
        {
            foreach (TileObject neighbour in neighbours)
            {
                if (neighbour != null && neighbour.owner != null)
                {
                    List<TileObject> tiles = new List<TileObject>();
                    if (!TraceBase(neighbour, tiles)) tiles.ForEach(x => x.SetOwner(null, false));
                }
            }
        }

        public void RefreshColor()
        {
            Color color = Owner != null ? (Owner.red ? Color.red : Color.green) : Color.gray;
            color.a = Player.player_on_turn.destinations.Is(this) || Player.player_on_turn.sources.Is(this) ? 1.0f : 0.5f;
            if (structure != null) color.b = 1.0f;
        }


        public Structure structure;

        public void RecalculateSourceAndDestinationProvision()
        {
            if (Owner != null)
            {
                Owner.sources.Set(this, true);
                Owner.opponent.sources.Set(this, false);

                foreach (Player player in Player.players) player.destinations.Set(this, false);
            }
            else
            {
                foreach (Player player in Player.players)
                {
                    player.sources.Set(this, false);
                    player.destinations.Set(this, neighbours.Any(x => x != null && x.Owner == player));
                }
            }

            foreach (TileObject neighbour in neighbours)
            {
                if (neighbour != null)
                {
                    foreach (Player player in Player.players)
                    {
                        player.destinations.Set(neighbour, neighbour.Owner == null && neighbour.neighbours.Any(x => x != null && x.Owner == player));
                    }
                }
            }
        }

        public static bool TraceBase(TileObject root, List<TileObject> explored)
        {

            if (root.structure is Structures.Base && (root.structure as Structures.Base).main) return true;
            explored.Add(root);

            IEnumerable<TileObject> next = root.neighbours.Where(x => x != null && x.Owner == root.Owner && !explored.Contains(x)).OrderByDescending(x => (x.position - explored.First().position).sqrMagnitude);
            foreach (TileObject tile in next)
            {
                if (TraceBase(tile, explored)) return true;
            }

            return false;
        }

        public static void Push(TileObject tile, Vector2Int direction)
        {
            int x = direction.x == 0 ? 2 : direction.x;
            int y = direction.y == 0 ? 1 : direction.y;
            Push(tile, x + y);

            tile.CheckNeighbourIntegrity();
        }

        static void Push(TileObject root, int i)
        {
            TileObject next = root.neighbours[i];
            if (next != null)
            {
                if (next.Owner != null) Push(next, i);
                next.SetOwner(root.Owner, true);
                root.SetOwner(null, false);
            }
        }

        public void SelectSource()
        {
            if (!Player.player_on_turn.sources.Is(this)) throw new Exception("Tried to select " + position + " - not a source.");
            Player.source = this;
        }

        public void SelectDestination()
        {
            if (!Player.player_on_turn.destinations.Is(this)) throw new Exception("Tried to move " + Player.source.position + " into " + position + " - not a destination.");

            bool free_tile = Player.source.Owner.free_tiles.Contains(Player.source);
            Player source_owner = Player.source.Owner;

            SetOwner(Player.player_on_turn, true);
            Player.source.SetOwner(null, true);
            Player.source = null;

            if (!free_tile || source_owner.free_tiles.Count == 0) Player.Next();
        }
    }
}
