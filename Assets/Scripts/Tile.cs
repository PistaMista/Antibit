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

        public void SelectSource()
        {
            if (!IsSource) throw new Exception("Tried to select " + position + " - not a source.");
            Player.source = this;
        }

        public void SelectDestination()
        {
            if (!IsDestination) throw new Exception("Tried to move " + Player.source.position + " into " + position + " - not a destination.");
            Player.source.owner = null;
            owner = Player.player_on_turn;
        }

        static void SelectGroup(Tile root, ref List<Tile> output)
        {
            output.Add(root);
            foreach (Tile tile in root.neighbours)
            {
                if (tile != null && tile.owner == root.owner && !output.Contains(tile))
                    SelectGroup(tile, ref output);
            }
        }
    }
}
