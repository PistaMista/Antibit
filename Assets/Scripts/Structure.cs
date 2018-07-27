using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        public byte startingCompleteness;
        public byte requiredCompleteness;
        public bool symmetrical;


        public Tile corner;
        public Vector2Int orientation;

        public static byte[] ghosts;
        public static void AddGhosts()
        {
            int ghostCount = 0;
            foreach (Structure prefab in Board.board.structurePrefabs)
            {
                ghostCount += (Board.board.size - prefab.footprint.x + 1) * (Board.board.size - prefab.footprint.y + 1) * (prefab.symmetrical ? 1 : 4);
            }

            ghosts = new byte[ghostCount];
            Debug.Log("Ghost count: " + ghostCount);
        }
        public static void ChangeGhost(int id, sbyte change)
        {

        }
    }
}