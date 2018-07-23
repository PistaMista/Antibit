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
        private Text text;

        private Player owner;
        public Player Owner
        {
            get
            {
                return owner;
            }
            set
            {
                if (owner != value)
                {
                    RecalculateSourceAndDestinationProvision();
                    owner = value;
                }
            }
        }



        public Structure structure;

        void Awake()
        {
            image = GetComponent<RawImage>();
            text = GetComponentInChildren<Text>();
        }

        public void RecalculateSourceAndDestinationProvision()
        {
            if (owner != null)
            {
                owner.sources.Set(this, true);
                owner.opponent.sources.Set(this, false);

                foreach (Player player in Player.players) player.destinations.Set(this, false);
            }
            else
            {
                foreach (Player player in Player.players)
                {
                    player.sources.Set(this, false);
                    player.destinations.Set(this, neighbours.Any(x => x != null && x.owner == player));
                }
            }

            foreach (Tile neighbour in neighbours)
            {
                if (neighbour != null)
                {
                    foreach (Player player in Player.players)
                    {
                        player.destinations.Set(neighbour, neighbour.owner == null && neighbour.neighbours.Any(x => x != null && x.owner == player));
                    }
                }
            }
        }

        public static void Push(Tile tile, Vector2Int direction)
        {
            int x = direction.x == 0 ? 2 : direction.x;
            int y = direction.y == 0 ? 1 : direction.y;
            Push(tile, x + y);

            Board.CheckIntegrity();
        }

        static void Push(Tile tile, int i)
        {
            Tile next = tile.neighbours[i];
            if (next != null)
            {
                if (next.Owner != null) Push(next, i);
                next.Owner = tile.Owner;
                tile.Owner = null;
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
            Owner = Player.player_on_turn;

            Player.source.Owner = null;
            Player.source = null;

            Board.CheckIntegrity();
            Player.Next();
        }
    }
}
