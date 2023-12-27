using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/WanderAction")]

public class WanderAction : ActionSO
{
    public override void Act(StateController controller)
    {
        Wander(controller);
    }

    public void Wander(StateController controller)
    {
        controller.stateTimer -= Time.deltaTime;

        if (controller.stateTimer <= 0f || controller.transform.position == controller.movePoint.position)
        {
            controller.movePoint.position = controller.GetRandomPointOnGraph();
            controller.GenerateStateTime();
        }

        controller.destinationSetter.target = controller.movePoint;

        Debug.DrawLine(controller.transform.position, controller.movePoint.position, Color.blue);


    }
}
