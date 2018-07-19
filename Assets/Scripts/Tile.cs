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
        public Structure structure;
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


            image.color = color;
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
