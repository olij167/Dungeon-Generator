using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/PursueAction")]

public class PursueAction : ActionSO
{
    public override void Act(StateController controller)
    {
        Pursue(controller);
    }

    public void Pursue(StateController controller)
    {
        if (controller.target != null)
        {

            controller.destinationSetter.target = controller.target.transform;

            Debug.DrawLine(controller.transform.position, controller.movePoint.position, Color.blue);

        }
       
        
    }
}
