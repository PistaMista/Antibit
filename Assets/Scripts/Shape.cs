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
            //0: L => R, B => T
            //1: B => T, R => L
            //2: R => L, T => B
            //3: T => B, L => R
            TileRequirement[][,] shapes = new TileRequirement[4][,];
            for (int i = 0; i < 4; i++)
            {
                shapes[i] = i % 2 == 0 ? new TileRequirement[footprint.x, footprint.y] : new TileRequirement[footprint.y, footprint.x];
            }

            int written_position = 0;
            for (int y = 0; y < footprint.y; y++)
            {
                for (int x = 0; x < footprint.x; x++)
                {
                    TileRequirement req = baseSequence[written_position++];
                    shapes[0][x, y] = req;
                    shapes[1][footprint.y - 1 - y, x] = req;
                    shapes[2][footprint.x - 1 - x, footprint.y - 1 - y] = req;
                    shapes[3][y, footprint.x - 1 - x] = req;
                }
            }

            List<TileRequirement[,]> variations = new List<TileRequirement[,]>();
            for (int i = 0; i < 4; i++)
            {
                TileRequirement[,] shape = shapes[i];
                Debug.Log(shape.VisualizationString(x => " " + x.ToString().First() + " "));
                if (!variations.Any(x => x.GetLength(0) == shape.GetLength(0) && x.GetLength(1) == shape.GetLength(1) && x.Cast<TileRequirement>().SequenceEqual(shape.Cast<TileRequirement>())))
                    variations.Add(shape);
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

            int[,] hook_count = new int[Board.board.size.x, Board.board.size.y];
            uint ghost_count = 0;
            foreach (Structure structure in Board.board.structurePrefabs)
            {
                foreach (Shape shape in structure.shapes)
                {
                    foreach (TileRequirement[,] variation in shape.variations)
                    {
                        Vector2Int bounds = new Vector2Int(Board.board.size.x - variation.GetLength(0), Board.board.size.y - variation.GetLength(1));
                        for (int bx = 0; bx <= bounds.x; bx++)
                        {
                            for (int by = 0; by <= bounds.y; by++)
                            {
                                for (int sx = 0; sx < variation.GetLength(0); sx++)
                                {
                                    for (int sy = 0; sy < variation.GetLength(1); sy++)
                                    {
                                        Vector2Int pos = new Vector2Int(bx + sx, by + sy);
                                        Board.board[pos.x, pos.y].structureGhosts[hook_count[pos.x, pos.y]] = ghost_count;
                                        Board.board[pos.x, pos.y].structureGhostInfluences[hook_count[pos.x, pos.y]++] = (sbyte)(1 - (int)variation[sx, sy]);
                                    }
                                }

                                ghost_count++;
                            }
                        }
                    }
                }
            }
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
