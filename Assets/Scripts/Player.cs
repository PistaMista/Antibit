using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Gameplay
{
    public class Player : ScriptableObject
    {
        public Tile origin;
        public bool ai = false;
        public bool red = false;


        public bool[,] sources;
        public bool[,] destinations;

        public Structure[] structures;

        public static Player[] players;
        public static Player player_on_turn;
        public static Tile source;
        public static void ReinitializePlayers(bool ai)
        {
            if (players != null) Array.ForEach(players, x => Destroy(x));

            players = new Player[2];
            for (int i = 0; i < 2; i++) players[i] = ScriptableObject.CreateInstance(typeof(Player)) as Player;

            //players[1].ai = ai;
            players[1].red = true;
        }

        public static void Next()
        {
            player_on_turn = players[player_on_turn == players[0] ? 1 : 0];
        }

        public void UseAI()
        {

        }
    }
}