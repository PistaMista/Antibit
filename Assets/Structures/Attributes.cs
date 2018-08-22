using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Structures
{
    /// <summary>
    /// Implies that a structure destroys tiles after deforming.
    /// </summary>
    interface UNSTABLE
    {
        void DestroyTilesAfterDeformation();
    }

    /// <summary>
    /// Specifies additional conditions for the formation of structures.
    /// </summary>
    interface CONDITIONAL
    {
        bool IsFormableFor(Player player);
        bool IsDeformable();
    }
}
