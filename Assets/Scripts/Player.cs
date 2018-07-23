﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Gameplay
{
    public class Player : ScriptableObject
    {
        public Player opponent;
        public Tile origin;
        public bool ai = false;
        public bool red = false;


        public bool[,] sources;
        public bool[,] destinations;

        public Structure[] structures;

        public static Player player_on_turn;
        public static Player[] players;
        public static Tile source;
        public static void ReinitializePlayers(bool ai)
        {
            if (players != null) Array.ForEach(players, x => Destroy(x));

            players = new Player[2];
            for (int i = 0; i < 2; i++) players[i] = ScriptableObject.CreateInstance(typeof(Player)) as Player;

            player_on_turn = players[0];
            player_on_turn.opponent = players[1];
            player_on_turn.opponent.opponent = player_on_turn;

            player_on_turn.opponent.red = true;
            //player_on_turn.opponent.ai = true;
        }

        public static void Next()
        {
            player_on_turn = player_on_turn.opponent;
        }

        public void UseAI()
        {

        }
    }
}