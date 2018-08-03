using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameplay
{
    public class Structure : MonoBehaviour
    {
        public Shape[] shapes;


        public Tile position;

        public void Form(int shape, int variation, int position)
        {

        }

        protected virtual void Form(Tile[][] markedTiles)
        {

        }

        public virtual void Deform()
        {

        }
    }
}