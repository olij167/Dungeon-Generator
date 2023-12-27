using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorMaster : MonoBehaviour
{
    public static DoorMaster instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    public class ConnectedDoors
    {
        public EnteranceDoor exteriorDoor;
        public EnteranceDoor interiorDoor;
    }

    public List<GameObject> doorPrefabList;
    public List<ConnectedDoors> connectedDoorsList;

    [field: ReadOnlyField] public List<GameObject> blockedPassages = new List<GameObject>();

    private LevelGenerator levelGenerator;

    private void Start()
    {
        levelGenerator = FindObjectOfType<LevelGenerator>();
    }

    public void SpawnInteriorEnteranceDoors() //replace a blocked passage with an enterance door for every element in connectedDoorsList
    {
        blockedPassages = new List<GameObject>();
        blockedPassages.AddRange(levelGenerator.blockedPassages);
        //Debug.Log("Starting blocked passages count: " + blockedPassages.Count);

        for (int i = 0; i < connectedDoorsList.Count; i++)
        {
            if (connectedDoorsList[i].interiorDoor == null)
            {
                int r = Random.Range(0, blockedPassages.Count);

                if (blockedPassages[r].activeInHierarchy)
                {
                    blockedPassages[r].SetActive(false);

                    GameObject newDoor = Instantiate(doorPrefabList[Random.Range(0, doorPrefabList.Count)], blockedPassages[r].transform.position, blockedPassages[r].transform.rotation, blockedPassages[r].transform.parent);

                    connectedDoorsList[i].interiorDoor = newDoor.GetComponentInChildren<EnteranceDoor>();
                    connectedDoorsList[i].interiorDoor.oppositeDoor = connectedDoorsList[i].exteriorDoor;
                    connectedDoorsList[i].exteriorDoor.oppositeDoor = newDoor.GetComponentInChildren<EnteranceDoor>();

                    blockedPassages.Remove(blockedPassages[r]);
                    //Debug.Log("blocked passages count: " + blockedPassages.Count);

                }
                //else
                //{
                //    Debug.Log("Duplicate selected, retrying");
                //    //SpawnInteriorEnteranceDoors(); // Re-run if they pick the same passage
                //    //break;
                //}
            }
        }

        //Recalculate all graphs
        //AstarPath.active.Scan();
    }
}
