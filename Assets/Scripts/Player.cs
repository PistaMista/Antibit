using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Gameplay
{
    public class Player : ScriptableObject
    {
        public Player opponent;
        public bool ai = false;
        public bool red = false;


        public bool[,] sources;
        public bool[,] destinations;
    }
}