using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class StateController : MonoBehaviour
{
    public State currentState;
    public AIStats aiStats;
    public Transform eyes;
    public State remainState;
    public GameObject prefab;

    [HideInInspector] public RichAI agent;
    [HideInInspector] public AIDestinationSetter destinationSetter;

    [HideInInspector] public float moveRange;
    public Transform movePointPrefab;
    public Transform movePoint;
    [HideInInspector] public bool movingToAPoint;
    [HideInInspector] public ObjectDetection detection;

    [HideInInspector] public GameObject target;

    public float stateTimer;


    private bool aiActive;
    void Awake()
    {
        agent = GetComponent<RichAI>();
        destinationSetter = GetComponent<AIDestinationSetter>();
        detection = GetComponent<ObjectDetection>();
        stateTimer = GenerateStateTime();

        //agent.maxSpeed = aiStats.maxSpeed;
        //agent.rotationSpeed = aiStats.turnSpeed;

        //agent.acceleration = aiStats.acceleration;
    }

    public void SetupAI (bool aiActivation, Vector3 waypoint)
    {
        if (movePoint == null)
        {
            movePoint = Instantiate(movePointPrefab, transform.position, Quaternion.identity);
            movePoint.parent = null;
        }

        movePoint.position = waypoint;
        aiActive = aiActivation;
        if (aiActive)
        {
            agent.enabled = true;
        }
        else agent.enabled = false;
    }

    private void Update()
    {
        if (!aiActive) return;

        currentState.UpdateState(this);
    }

    public Vector3 GetRandomPointOnGraph()
    {
        // Works for ANY graph type, however this is much slower
        var graph = AstarPath.active.data.graphs[0];
        // Add all nodes in the graph to a list
        List<GraphNode> nodes = new List<GraphNode>();
        graph.GetNodes((System.Action<GraphNode>)nodes.Add);
        GraphNode randomNode = nodes[Random.Range(0, nodes.Count)];

        // Use the center of the node as the destination for example
        var destination1 = (Vector3)randomNode.position;
        // Or use a random point on the surface of the node as the destination.
        // This is useful for navmesh-based graphs where the nodes are large.
        return randomNode.RandomPointOnSurface();
    }

    public float GenerateStateTime()
    {
        stateTimer = Random.Range(aiStats.minStateTime, aiStats.maxStateTime);
        return stateTimer;
    }

    private void OnDrawGizmos()
    {
        if (currentState != null && eyes != null)
        {
            Gizmos.color = currentState.sceneGizmoColor;
            Gizmos.DrawWireSphere(eyes.position, aiStats.lookSphere);
        }
    }

    public void TransitionToState(State nextState)
    {
        if (nextState != remainState)
        {
            currentState = nextState;
        }
    }
}
