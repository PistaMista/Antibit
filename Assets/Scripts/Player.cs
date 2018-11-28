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

        public List<Structure> structures;
        public List<Tiles.TileObject> free_tiles;

        void Awake()
        {
            sources = new bool[Board.board.size.x, Board.board.size.y];
            destinations = new bool[Board.board.size.x, Board.board.size.y];
            structures = new List<Structure>();
            free_tiles = new List<Tiles.TileObject>();

        }

        public static Player player_on_turn;
        public static Player[] players;
        public static Tiles.TileObject source;
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
            for (int x = 0; x < Board.board.size.x; x++)
            {
                for (int y = 0; y < Board.board.size.y; y++)
                {
                    Board.board[x, y].RefreshColor();
                }
            }

            player_on_turn.structures.ForEach(x => x.Activate());
        }

        public void UseAI()
        {

        }
    }
}