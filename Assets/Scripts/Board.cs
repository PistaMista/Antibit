﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class Board : MonoBehaviour
    {

        public void Renew()
        {

        }

        void Update()
        {
            if (Input.anyKeyDown) Renew();
        }
    }
}