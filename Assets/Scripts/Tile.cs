using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class Tile : MonoBehaviour
    {

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
    }
}
