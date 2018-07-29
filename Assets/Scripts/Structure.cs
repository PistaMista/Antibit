using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameplay
{
    public class Structure : MonoBehaviour
    {
        public enum Construction
        {
            PRESENT,
            MISSING,
            ANY
        }
        public Vector2Int footprint;
        public Construction[] construction;
        public Construction[][,] shapes;
        public byte startingCompleteness;
        public byte requiredCompleteness;


        public Tile corner;
        public Vector2Int orientation;

        void Awake()
        {
            construction = null;
            shapes = null;
        }

        public void CalculateShapes()
        {
            List<Construction[,]> shapes = new List<Construction[,]>();
            for (int o = 0; o < 4; o++)
            {
                bool vertical = o < 2;
                bool flipped = o % 2 == 1;
                Construction[,] shape = new Construction[vertical ? footprint.x : footprint.y, vertical ? footprint.y : footprint.x];

                int written_position = 0;

                for (int x = vertical ? 0 : footprint.x - 1; vertical ? (x < footprint.x) : (x >= 0); x += vertical ? 1 : -1)
                    for (int y = vertical ? 0 : footprint.y - 1; vertical ? (y < footprint.y) : (y >= 0); y += vertical ? 1 : -1)
                        shape[flipped ? y : x, flipped ? x : y] = construction[written_position++];

                Debug.Log(shape.VisualizationString(x => " " + x.ToString().First() + " "));

                if (!shapes.Contains(shape)) shapes.Add(shape);
            }

            this.shapes = shapes.ToArray();
        }

        public static byte[] ghosts;
        public static void AddGhosts()
        {
            int ghostCount = 0;
            foreach (Structure prefab in Board.board.structurePrefabs)
            {
                foreach (Construction[,] shape in prefab.shapes)
                {
                    ghostCount += Mathf.Clamp((Board.board.size.x - shape.GetLength(0) + 1) * (Board.board.size.y - shape.GetLength(1) + 1), 0, int.MaxValue);
                }
            }

            ghosts = new byte[ghostCount];
            Debug.Log("Ghost count: " + ghostCount);

            foreach (Structure prefab in Board.board.structurePrefabs)
            {
                foreach (Construction[,] shape in prefab.shapes)
                {

                }
            }
        }
        public static void ChangeGhost(int id, sbyte change)
        {

        }
    }
}