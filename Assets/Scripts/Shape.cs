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
            for (int o = 0; o < 4; o++)
            {
                bool vertical = o < 2;
                bool flipped = o % 2 == 1;
                TileRequirement[,] shape = new TileRequirement[vertical ? footprint.x : footprint.y, vertical ? footprint.y : footprint.x];

                int written_position = 0;

                for (int x = vertical ? 0 : footprint.x - 1; vertical ? (x < footprint.x) : (x >= 0); x += vertical ? 1 : -1)
                    for (int y = vertical ? 0 : footprint.y - 1; vertical ? (y < footprint.y) : (y >= 0); y += vertical ? 1 : -1)
                        shape[flipped ? y : x, flipped ? x : y] = baseSequence[written_position++];

                Debug.Log(shape.VisualizationString(x => " " + x.ToString().First() + " "));

                if (!variations.Contains(shape)) variations.Add(shape);
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
            foreach (Structure prefab in Board.board.structurePrefabs)
            {
                foreach (TileRequirement[,] shape in prefab.shape.variations)
                {

                }
            }
        }
        public static void ChangeGhostCompleteness(uint id, sbyte change, bool invert)
        {
            ghosts[id] = (byte)(invert ? ghosts[id] - change : ghosts[id] + change);
        }
    }
}
