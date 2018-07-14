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

        public int cost;
        public Player owner;
        public int Cost
        {
            get
            {
                return cost;
            }
            set
            {
                cost = value;
                text.text = value.ToString();
                UpdateColor();
            }
        }
        public Player Owner
        {
            get
            {
                return owner;
            }
            set
            {
                owner = value;
                UpdateColor();
            }
        }

        void Awake()
        {
            image = GetComponent<RawImage>();
            text = GetComponentInChildren<Text>();
        }

        public void UpdateColor()
        {
            Color color = owner != null ? owner.red ? Color.red : Color.green : Color.white;

            if (owner == Player.player_on_turn) color.a = 1.0f;
            else if (cost <= Player.bit_cap && cost > 0) color.a = 0.75f;
            else if (owner != null) color.a = 0.5f;
            else color.a = 0.1f;

            image.color = color;
        }

        public void RecalculateCost()
        {
            if (Player.player_on_turn == Owner) Cost = 0;
            else
            {
                int player_neighbours = neighbours.Count(x => x != null && x.Owner == Player.player_on_turn);
                if (Owner != null)
                {
                    Cost = Mathf.Clamp(Player.bit_cap + 1 - player_neighbours * 2, 1, int.MaxValue);
                }
                else
                {
                    Cost = player_neighbours;
                }
            }
        }

        public void Take()
        {
            bool takeover = Owner != null;
            Player.player_on_turn.bits -= cost;
            Cost = 0;
            Owner = Player.player_on_turn;

            if (takeover)
            {
                foreach (Tile neighbour in neighbours)
                {
                    if (neighbour != null && neighbour.Owner != null && neighbour.Owner != Owner)
                    {
                        List<Tile> group = new List<Tile>();
                        SelectGroup(neighbour, ref group);

                        if (!group.Contains(neighbour.Owner.origin))
                        {
                            group.ForEach(x => x.Owner = null);
                        }
                    }
                }
            }

            foreach (Tile neighbour in neighbours)
            {
                if (neighbour != null) neighbour.RecalculateCost();
            }

            //If there are no more expandable tiles go to the next player.
            for (int x = 0; x < Board.board.size.x; x++)
            {
                for (int y = 0; y < Board.board.size.y; y++)
                {
                    Tile tile = Board.board[x, y];
                    if (tile.Cost != 0 && tile.Cost <= Player.player_on_turn.bits) return;
                    if (x == Board.board.size.x - 1 && y == Board.board.size.y - 1) Player.Next();
                }
            }
        }

        static void SelectGroup(Tile root, ref List<Tile> output)
        {
            output.Add(root);
            foreach (Tile tile in root.neighbours)
            {
                if (tile != null && tile.Owner == root.Owner && !output.Contains(tile))
                    SelectGroup(tile, ref output);
            }
        }
    }
}
