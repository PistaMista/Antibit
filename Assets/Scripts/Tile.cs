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
        RawImage image;
        Text text;

        public Player owner;
        public bool IsSource
        {
            get
            {
                return Player.player_on_turn.sources[position.x, position.y];
            }
            set
            {
                Player.player_on_turn.sources[position.x, position.y] = value;
            }
        }
        public bool IsDestination
        {
            get
            {
                return Player.player_on_turn.destinations[position.x, position.y];
            }
            set
            {
                Player.player_on_turn.sources[position.x, position.y] = value;
            }
        }
        public Structure structure;

        void Awake()
        {
            image = GetComponent<RawImage>();
            text = GetComponentInChildren<Text>();
        }

        public static void Push(Tile tile, Vector2Int direction)
        {
            int x = direction.x == 0 ? 2 : direction.x;
            int y = direction.y == 0 ? 1 : direction.y;
            Push(tile, x + y);
        }

        public static void Push(Tile tile, int i)
        {
            Tile next = tile.neighbours[i];
            if (next != null)
            {
                if (next.owner != null) Push(next, i);
                next.owner = tile.owner;
                tile.owner = null;
            }
        }

        public void SelectSource()
        {
            if (!IsSource) throw new Exception("Tried to select " + position + " - not a source.");
            Player.source = this;
        }

        public void SelectDestination()
        {
            if (!IsDestination) throw new Exception("Tried to move " + Player.source.position + " into " + position + " - not a destination.");
            owner = Player.player_on_turn;

            Player.source.owner = null;
            Player.source = null;
        }
    }
}
