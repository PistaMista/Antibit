using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Gameplay
{
    public class Tile : MonoBehaviour
    {
        public Vector2Int position;
        public Tile[] neighbours;
        private RawImage image;

        private Player owner;
        public Player Owner
        {
            get
            {
                return owner;
            }
        }

        public static Action<Tile, Player, Player> OnTileOwnershipChange;

        public void SetOwner(Player player, bool checkIntegrity)
        {
            if (owner != player)
            {
                Player last_owner = owner;
                owner = player;

                RecalculateSourceAndDestinationProvision();
                if (OnTileOwnershipChange != null) OnTileOwnershipChange(this, last_owner, owner);


                if (checkIntegrity && last_owner != null)
                {
                    foreach (Tile neighbour in neighbours)
                    {
                        if (neighbour != null && neighbour.owner == last_owner)
                        {
                            List<Tile> tiles = new List<Tile>();
                            if (!TraceOrigin(neighbour, tiles)) tiles.ForEach(x => x.SetOwner(null, false));
                        }
                    }
                }
            }
        }

        public void RefreshColor()
        {
            Color color = Owner != null ? (Owner.red ? Color.red : Color.green) : Color.gray;
            color.a = Player.player_on_turn.destinations.Is(this) || Player.player_on_turn.sources.Is(this) ? 1.0f : 0.5f;
            if (structure != null) color.b = 1.0f;
            image.color = color;
        }


        public Structure structure;

        void Awake()
        {
            image = GetComponent<RawImage>();
        }

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

            foreach (Tile neighbour in neighbours)
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

        public static bool TraceOrigin(Tile root, List<Tile> explored)
        {

            if (root.structure is Structures.Base) return true;
            explored.Add(root);

            IEnumerable<Tile> next = root.neighbours.Where(x => x != null && x.Owner == root.Owner && !explored.Contains(x)).OrderByDescending(x => (x.position - explored.First().position).sqrMagnitude);
            foreach (Tile tile in next)
            {
                if (TraceOrigin(tile, explored)) return true;
            }

            return false;
        }

        public static void Push(Tile tile, Vector2Int direction)
        {
            int x = direction.x == 0 ? 2 : direction.x;
            int y = direction.y == 0 ? 1 : direction.y;
            Push(tile, x + y);

            tile.SetOwner(null, true);
        }

        static void Push(Tile root, int i)
        {
            Tile next = root.neighbours[i];
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
            SetOwner(Player.player_on_turn, true);

            Player.source.SetOwner(null, true);
            Player.source = null;

            Player.Next();
        }
    }
}
