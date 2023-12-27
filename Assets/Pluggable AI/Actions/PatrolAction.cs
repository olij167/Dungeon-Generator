using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/PatrolAction")]

public class PatrolAction : ActionSO
{
    Connector[] allConnectors = FindObjectsOfType<Connector>();

    public int connectorPatrolCount;

    private void Awake()
    {
        connectorPatrolCount = Random.Range(0, allConnectors.Length);
    }
    public override void Act(StateController controller)
    {
        Patrol(controller);
    }

    public void Patrol(StateController controller)
    {
        controller.stateTimer -= Time.deltaTime;

        if (controller.stateTimer <= 0f || controller.transform.position == controller.movePoint.position)
        {
            controller.movePoint.position = NextPatrolPoint();
            controller.GenerateStateTime();
        }

        controller.destinationSetter.target = controller.movePoint;

        Debug.DrawLine(controller.transform.position, controller.movePoint.position, Color.blue);


    }

    public Vector3 NextPatrolPoint()
    {
        if (connectorPatrolCount + 1 < allConnectors.Length)
        {
            connectorPatrolCount++;
        }
        else connectorPatrolCount = 0;

        return allConnectors[connectorPatrolCount].transform.position;
    }
}
