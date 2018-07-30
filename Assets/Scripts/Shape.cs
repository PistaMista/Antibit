using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "Shape", menuName = "Shape", order = 1)]
    public class Shape : ScriptableObject
    {
        public enum TileRequirement
        {
            OWNED,
            ANY,
            NONE
        }
        public Vector2Int footprint;
        public TileRequirement[] baseSequence;
        TileRequirement[][,] variations;
        byte startingCompleteness;
        byte requiredCompleteness;

        public static void InitializeAll()
        {
            foreach (Structure structure in Board.board.structurePrefabs)
            {
                foreach (Shape shape in structure.shapes)
                {
                    shape.CalculateVariations();
                    shape.CalculateRequiredCompleteness();
                }
            }
        }
        void CalculateVariations()
        {
            List<TileRequirement[,]> variations = new List<TileRequirement[,]>();
            //0: L => R, B => T
            //1: B => T, R => L
            //2: R => L, T => B
            //3: T => B, L => R
            for (int o = 0; o < 4; o++)
            {
                bool vertical = o % 2 == 0;

                bool top_to_bottom = o >= 2;
                bool right_to_left = o == 1 || o == 2;

                TileRequirement[,] shape = new TileRequirement[vertical ? footprint.x : footprint.y, vertical ? footprint.y : footprint.x];

                int written_position = 0;

                for (int a = 0; a < footprint.x; a++)
                {
                    for (int b = 0; b < footprint.y; b++)
                    {
                        int x = right_to_left ? footprint.x - a - 1 : a;
                        int y = top_to_bottom ? footprint.y - b - 1 : b;
                        shape[vertical ? x : y, vertical ? y : x] = baseSequence[written_position++];
                    }
                }


                Debug.Log(shape.VisualizationString(x => " " + x.ToString().First() + " "));

                if (!variations.Any(x =>
                {
                    return shape.GetLength(0) == x.GetLength(0) &&
                    shape.GetLength(1) == x.GetLength(1) &&
                    shape.Cast<TileRequirement>().SequenceEqual(x.Cast<TileRequirement>());
                }
                )) variations.Add(shape);
            }

            this.variations = variations.ToArray();
        }

        void CalculateRequiredCompleteness()
        {
            startingCompleteness = (byte)baseSequence.Count(x => x == TileRequirement.NONE);
            requiredCompleteness = (byte)baseSequence.Count(x => x == TileRequirement.OWNED);
        }


        static byte[] ghosts;
        public static void InitializeGhosts()
        {
            InitializeGhostCompletenessArray();
            InitializeGhostCompletenessChangingTileArrays();

        }

        static void InitializeGhostCompletenessArray()
        {
            int ghostCount = 0;
            foreach (Structure structure in Board.board.structurePrefabs)
                foreach (Shape shape in structure.shapes)
                    foreach (TileRequirement[,] variation in shape.variations)
                        ghostCount += Mathf.Clamp((Board.board.size.x - variation.GetLength(0) + 1) * (Board.board.size.y - variation.GetLength(1) + 1), 0, int.MaxValue);

            ghosts = new byte[ghostCount];
            Debug.Log("Ghost count: " + ghostCount);
        }

        static void InitializeGhostCompletenessChangingTileArrays()
        {
            int[,] edgeDistances = new int[Board.board.size.x, Board.board.size.y];
            for (int x = 0; x < Board.board.size.x; x++)
            {
                for (int y = 0; y < Board.board.size.y; y++)
                {
                    edgeDistances[x, y] = (Mathf.Abs(Mathf.Abs(x - Board.board.center.x) - Board.board.center.x) + 1) * (Mathf.Abs(Mathf.Abs(y - Board.board.center.y) - Board.board.center.y) + 1);
                }
            }

            int[,] arraySizes = new int[Board.board.size.x, Board.board.size.y];
            foreach (Structure structure in Board.board.structurePrefabs)
            {
                foreach (Shape shape in structure.shapes)
                {
                    int maxOverlap = shape.footprint.x * shape.footprint.y;
                    int variations = shape.variations.Length;

                    for (int x = 0; x < Board.board.size.x; x++)
                    {
                        for (int y = 0; y < Board.board.size.y; y++)
                        {
                            arraySizes[x, y] += Mathf.Clamp(edgeDistances[x, y], 0, maxOverlap) * variations;
                        }
                    }
                }
            }

            for (int x = 0; x < Board.board.size.x; x++)
            {
                for (int y = 0; y < Board.board.size.y; y++)
                {
                    int size = arraySizes[x, y];
                    Tile tile = Board.board[x, y];

                    tile.structureGhosts = new uint[size];
                    tile.structureGhostInfluences = new sbyte[size];
                }
            }

        }
        public static void ChangeGhostCompleteness(uint id, sbyte change, bool invert)
        {
            ghosts[id] = (byte)(invert ? ghosts[id] - change : ghosts[id] + change);
        }
    }
}
