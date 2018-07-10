using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class Tile : MonoBehaviour
    {

        RawImage image;
        Color color
        {
            get
            {
                return image.color;
            }
            set
            {
                image.color = value;
            }
        }
        Text text;
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
            }
        }

        int cost;
        int order;
        Player owner;

        void Start()
        {
            image = GetComponent<RawImage>();
            text = GetComponentInChildren<Text>();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
