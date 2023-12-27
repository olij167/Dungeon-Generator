using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnteranceDoor : MonoBehaviour
{

    // All doors lead back to iD 1 (except iD 1, it goes to the first iD 2)
    // May need to change in future if you want more than 1 enterance

    public EnteranceDoor oppositeDoor;
    public bool isInTrigger;
    private GameObject player;
    public Transform enterPos;

    //List<EnteranceDoor> enteranceList = new List<EnteranceDoor>();

    //private void Start()
    //{
    //    foreach(EnteranceDoor door in FindObjectsOfType<EnteranceDoor>())
    //    {
    //        enteranceList.Add(door);

    //        if (iD != 1 && door.iD == 1)
    //        {
    //            oppositeDoor = door;
    //            if (oppositeDoor.oppositeDoor == null)
    //            { 
    //                oppositeDoor.oppositeDoor = this;
    //            }
    //        }
    //        else if (iD == 1 && door.iD != 1)
    //        {
    //            oppositeDoor = door;
    //        }
    //    }
    //}

    private void Update()
    {
        if (isInTrigger && Input.GetButtonDown("Interact"))
        {
            WarpEnter(oppositeDoor.enterPos.position);
            //WarpEnter(!iD);
        }
    }

    void WarpEnter(Vector3 enterPoint)
    {
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<CharacterController>().transform.position = enterPoint;
        isInTrigger = false;
        player.GetComponent<CharacterController>().enabled = true;

        Debug.Log("Entering");
    }

    //void WarpEnter(int doorNum)
    //{
    //    for (int i = 0; i < enteranceList.Count; i++)
    //    {
    //        if (enteranceList[i].iD == doorNum)
    //        {
    //            player.GetComponent<CharacterController>().enabled = false;
    //            player.GetComponent<CharacterController>().transform.position = enteranceList[i].enterPos.position;
    //            isInTrigger = false;
    //            player.GetComponent<CharacterController>().enabled = true;

    //            Debug.Log("Entering");
    //        }
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInTrigger = true;
            player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInTrigger = false;
        }
    }

}
