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
        public Tile corner;
        public Vector2Int orientation;

        public static void TryFormAny()
        {

        }



        private void TryForm()
        {

        }

        protected bool CheckConditions()
        {
            return true;
        }
    }
}