using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Gameplay
{
    public class Player : ScriptableObject
    {
        public bool ai = false;
        public bool red = false;
        public int bits = bit_cap;
        public const int bit_cap = 3;
        public static Player[] players;
        public static Player player_on_turn;
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
            player_on_turn.bits = bit_cap;
            for (int x = 0; x < Board.board.size.x; x++)
                for (int y = 0; y < Board.board.size.y; y++)
                    Board.board[x, y].RecalculateCost();
        }
    }
}