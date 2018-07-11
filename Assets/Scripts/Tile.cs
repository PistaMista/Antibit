using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Gameplay
{
    public class Tile : MonoBehaviour
    {
        public const int exploration_cost = 1;
        public const int takeover_cost = 2;
        public Vector2Int position;
        public Tile[] neighbours
        {
            get
            {
                Tile[] result = new Tile[4];
                for (int i = 0; i < 4; i++)
                {
                    Vector2Int p = position + new Vector2Int((i - 1) % 2, (i - 2) % 2);
                    if (p.x >= 0 && p.x < Board.board.size.x && p.y >= 0 && p.y < Board.board.size.y)
                        result[i] = Board.board[p.x, p.y];
                }

                return result;
            }
        }
        RawImage image;
        Text text;

        public int order;
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
            else if (cost == 0) color.a = 0.5f;
            else if (cost > Player.bit_cap) color.a = 0.1f;
            else if (cost <= Player.player_on_turn.bits) color.a = 0.75f;

            image.color = color;
        }

        public void RecalculateCost()
        {
            if (cost <= Player.bit_cap)
            {
                if (Player.player_on_turn == Owner) Cost = 0;
                else if (neighbours.Any(x => x != null && x.Owner == Player.player_on_turn))
                {
                    if (Owner != null) Cost = takeover_cost;
                    else Cost = exploration_cost;
                }
            }
        }

        public void Take()
        {
            Player.player_on_turn.bits -= cost;
            Cost = 0;
            Owner = Player.player_on_turn;

            foreach (Tile tile in neighbours)
                if (tile != null) tile.RecalculateCost();
        }
    }
}
