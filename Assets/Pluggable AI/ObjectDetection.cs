using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDetection : MonoBehaviour
{
   // object avoidance variables
    public StateController controller;
    public LayerMask ignoreMasks;
    public List<int> layerIndex;

    Vector3 target;
    bool obstacleAvoid;
    public bool targetInLookSphere;

    private Transform obstacleInPath;

    // object detection variables
    public List<GameObject> detectedObjects;

    public LayerMask interestingLayers;

    private void Awake()
    {
        for (int i = 0; i < layerIndex.Count; i++)
            Physics.IgnoreLayerCollision(9, layerIndex[i]); // dont detect these layers
    }
    void FixedUpdate()
    {
        DetectObjects(detectedObjects);
        ObstacleAvoidance();
    }

    public List<GameObject> DetectObjects(List<GameObject> objectsAroundAI)
    {
        
        RaycastHit hit;

        if (Physics.SphereCast(controller.eyes.position, controller.aiStats.lookSphere, transform.forward, out hit, controller.aiStats.lookSphere, interestingLayers) && !detectedObjects.Contains(hit.transform.gameObject))
        {
            Debug.DrawLine(transform.position, hit.point, Color.cyan);
            objectsAroundAI.Add(hit.transform.gameObject);

            if (controller.target == null)
            {
                if (hit.transform.gameObject.CompareTag("Player"))
                {
                    controller.target = hit.transform.gameObject;
                }

                //if (hit.transform.gameObject.CompareTag("Bee"))
                //{
                //    controller.target = (hit.transform.gameObject);
                //    controller.stateTimer = 30f;
                //}

                //if (hit.transform.gameObject.CompareTag("Flower"))
                //{
                //    controller.target = hit.transform.gameObject;
                //    controller.stateTimer = 15f;
                //}

                //if (hit.transform.gameObject.CompareTag("Litter"))
                //{
                //    controller.target = hit.transform.gameObject;
                //}

                //if (hit.transform.gameObject.CompareTag("NPC"))
                //{
                //    controller.target = hit.transform.gameObject;
                //}
            }
        }

        

        for (int i = 0; i < objectsAroundAI.Count; i++)
        {
            if (objectsAroundAI[i] != (Physics.SphereCast(controller.eyes.position, controller.aiStats.lookSphere, transform.forward, out hit, controller.aiStats.lookSphere, interestingLayers)))
            {
                objectsAroundAI.Remove(detectedObjects[i]);
            }
        }

        return objectsAroundAI;
    }

    private void ObstacleAvoidance()
    {
        if (controller.movingToAPoint)
        {
            target = controller.movePoint.position;
        }

        RaycastHit hit;
        Vector3 dir = (target - transform.position).normalized;

        if (Physics.SphereCast(controller.eyes.position, controller.aiStats.lookSphere, transform.forward, out hit, controller.aiStats.lookSphere, ignoreMasks))
        {
            obstacleAvoid = true;
            Debug.DrawLine(transform.position, hit.point, Color.red);
            controller.agent.isStopped = true;
            //controller.agent.ResetPath();

            if (hit.transform != transform)
            {
                obstacleInPath = hit.transform;
                //Debug.Log("Obstacle in Path = " + hit.transform.gameObject.name);
                dir += hit.normal * controller.agent.rotationSpeed;

                //Debug.Log("Moving around an object");
            }
        }

        if (obstacleInPath != null)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 toOther = obstacleInPath.position - transform.position;
            if (Vector3.Dot(forward, toOther) < 0)
            {
                //print("The other transform is behind me!");
                //Debug.Log("Back on Navigation! unit - " + gameObject.name);
                obstacleAvoid = false; // don't let Unity nav and our avoidance nav fight, character does odd things
                obstacleInPath = null; // Hakuna Matata
                //controller.agent.ResetPath();
                controller.destinationSetter.target = controller.movePoint;
                controller.agent.isStopped = true; // Unity nav can resume movement control
            }
        }

        if (obstacleAvoid)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime);
            transform.position += transform.forward * controller.agent.maxSpeed * Time.deltaTime;
        }
    }


}
