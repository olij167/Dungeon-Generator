using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/SeeTargetDecision")]

public class SeeTargetDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        return SeeTarget(controller);
    }

    private bool SeeTarget(StateController controller)
    {
        if (controller.target != null)
        {
            return true;
        }
        else return false;
    }
}
