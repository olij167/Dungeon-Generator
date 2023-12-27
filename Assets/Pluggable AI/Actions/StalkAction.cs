using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/StalkAction")]
public class StalkAction : ActionSO
{
    public float stalkDistance = 10f;

    public override void Act(StateController controller)
    {
        Stalk(controller);
    }

    public void Stalk (StateController controller)
    {
        //controller.destinationSetter.target = controller.target.transform +- new Vector3();
        //controller.movePoint =  

        if (Vector3.Distance(controller.target.transform.position, controller.transform.position) <= stalkDistance)
        {
            Vector3 dir = (controller.target.transform.position - controller.transform.position) / (controller.target.transform.position - controller.transform.position).magnitude;

            controller.movePoint.position = controller.transform.position + dir * stalkDistance * Time.deltaTime;
        }
        else
        {
            controller.movePoint.position = controller.transform.position;
            controller.transform.LookAt(controller.target.transform);
        }
        Debug.DrawLine(controller.transform.position, controller.movePoint.position, Color.blue);


    }
}
