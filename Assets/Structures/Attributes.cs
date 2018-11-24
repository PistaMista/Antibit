using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Structures
{
    /// <summary>
    /// Implies that a structure destroys tiles after deforming.
    /// </summary>
    interface SA_UNSTABLE
    {
        void DestroyTilesAfterDeformation();
    }

    /// <summary>
    /// Specifies additional conditions for the formation of structures.
    /// </summary>
    interface SA_CONDITIONAL
    {
        bool IsFormableFor(Player player);
        bool IsDeformable();
    }
}

namespace Gameplay.Tiles
{
    public interface TA_SOLID
    {

    }
}
